using System;

namespace lulzbot
{
    /// <summary>
    /// General console input/output wrapper.
    /// </summary>
    public class ConIO
    {
        // Since this is a static context, we need a static object to use as a lock.
        private static readonly object OutputLock = new object();

        // You can change the global timestamp and namespace colors here.
        private const ConsoleColor TimestampColor = ConsoleColor.Gray;
        private const ConsoleColor NamespaceColor = ConsoleColor.Blue;

        /// <summary>
        /// Static method to write a line to the console.
        /// </summary>
        /// <param name="output">Message to print.</param>
        /// <param name="ns">Namespace. Defaults to "Bot". Usually a #channel</param>
        public static void Write (String output, String ns = "Bot")
        {
            if (!Program.Running && !Program.Debug) return; // No need to output any queued data after this.
            if (Program.NoDisplay.Contains(ns.ToLower())) return;

            // We're going to use colors, because why not? Makes it look nice.
            // Of course, in a threaded environment, colors can get messed up and
            //  not display correctly. So, we lock output to keep the order correct.
            lock (OutputLock)
            {
                Console.ForegroundColor = TimestampColor;
                Console.Write("{0} ", Timestamp());
                Console.ForegroundColor = NamespaceColor;
                Console.Write("[{0}] ", ns);
                Console.ResetColor();
                Console.WriteLine(output);
            }

            // Log output event
            if ((ns.StartsWith("#") || ns.StartsWith("@")))
                Events.CallSpecialEvent("log_msg", new object[] { Program.Bot, ns, output });
        }

        /// <summary>
        /// Outputs a warning.
        /// </summary>
        /// <param name="where">What file/function triggered this warning.</param>
        /// <param name="output">warning message</param>
        public static void Warning (String where, String output)
        {
            if (!Program.Running && !Program.Debug) return; // No need to output any queued data after this.

            lock (OutputLock)
            {
                Console.ForegroundColor = TimestampColor;
                Console.Write("{0} ", Timestamp());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[WARNING] ({0}) ", where);
                Console.ResetColor();
                Console.WriteLine(output);
            }
        }

        /// <summary>
        /// Outputs a notice.
        /// </summary>
        /// <param name="output">notice message</param>
        public static void Notice (String output)
        {
            if (!Program.Running && !Program.Debug) return; // No need to output any queued data after this.

            lock (OutputLock)
            {
                Console.ForegroundColor = TimestampColor;
                Console.Write("{0} ", Timestamp());
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("*!* [NOTICE] ");
                Console.ResetColor();
                Console.WriteLine(output);
            }
        }

        /// <summary>
        /// Static method for prompting the user for input and returning it.
        /// </summary>
        /// <param name="prompt">The prompt to display.</param>
        /// <param name="ns">Namespace. Defaults to "Bot.". Usually a #channel</param>
        /// <returns>The user's input.</returns>
        public static String Read (String prompt, String ns = "Bot")
        {
            // Buffer for the received input.
            String input = String.Empty;

            // Again, colors. Looks nicer. Still needs locking for the colors.
            lock (OutputLock)
            {
                Console.ForegroundColor = TimestampColor;
                Console.Write("{0} ", Timestamp());
                Console.ForegroundColor = NamespaceColor;
                Console.Write("[{0}] ", ns);
                Console.ResetColor();
                Console.Write("{0}: ", prompt);
                input = Console.ReadLine();
            }
            return input;
        }

        /// <summary>
        /// Simple method to create an output timestamp.
        /// </summary>
        /// <returns>Timestamp in string format.</returns>
        private static String Timestamp ()
        {
            DateTime time = DateTime.Now;
            return String.Format("[{0}:{1}:{2}]",
                time.Hour.ToString().PadLeft(2, '0'),
                time.Minute.ToString().PadLeft(2, '0'),
                time.Second.ToString().PadLeft(2, '0'));
        }
    }
}
