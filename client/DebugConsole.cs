#define DEBUG

namespace Debug {
    public static class DebugConsole {
        public static void WriteLine(string line) {
            #if DEBUG
            Console.WriteLine($"[{DateTime.Now}] - {line}");
            #endif
        }
    }
}