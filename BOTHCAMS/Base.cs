using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace BOTHCAMS {
    class Base {//By Kristian Dimitrov, V5
    //TODO: Make camera autofind 
    //      Make program not fail if entering in IP that exists but is not the camera (maybe done?)
    //      Detect IP conflicts that would break the program (maybe done?)
    //      Fix bug to do with needing to login/struggling with conflicts? (maybe something do with restarting?) (maybe done?)
    //      Add sync time button functionality (if possible)

        public static string defaultThermalIp = "192.168.1.123";
        public static string defaultDayIp = "192.168.1.100";

        public static string newThermalIp = "192.168.1.101";
        public static string newDayIp = "192.168.1.100";
        static void Main(string[] args) {
            DayCam(AdjustTime(), DateTime.Now.ToString("dd" + "/" + "MM" + "/" + "yyyy"));
            ThermalCam();
            Final();
        }

        private static void DayCam(string t, string d) {
            Console.WriteLine("DAYLIGHT CAMERA SETTINGS WILL BE BEING APPLIED --------------");

            string daylightIP = "http://" + CheckPing(defaultDayIp);
            
            Console.WriteLine("Applying Daylight Camera Settings To: " + daylightIP);

            try {
                while (!Daylight.DoDaylightRequestsAsync(daylightIP, t, d).Result) { //daylight camera loop
                    Console.WriteLine("Loop------------------");
                }
            } catch {
                Console.WriteLine("An error has occured with applying the daylight camera settings. \nPress ENTER to close the program");
                Console.Read();
            }
        }

        private static void ThermalCam() {
            Console.WriteLine("THERMAL CAMERA SETTINGS WILL BE BEING APPLIED --------------");

            string thermalIP = "http://" + CheckPing(defaultThermalIp);

            Console.WriteLine("Applying Thermal Camera Settings To: " + thermalIP);

            try {
                while (!Thermal.DoThermalRequestsAsync(thermalIP).Result) { //thermal camera loop
                    Console.WriteLine("Loop------------------");
                }
            } catch {
                Console.WriteLine("An error has occured with applying the thermal camera settings. \nPress ENTER to close the program");
                Console.Read();
            }
        }

        public static string CheckPing(string ip) { //check if ip is camera
            string checkIp = ip;
            HttpClient cl = new HttpClient();
            cl.Timeout = new TimeSpan(0, 0, 0, 5);
            bool ipValid = false;
            Console.WriteLine("CORE: Looking For Camera At: " + checkIp);
            while (!ipValid) {
                for (int i = 0; i < 3; i++) {
                    if (PingHost(checkIp)) {
                        Console.WriteLine("CORE: " + checkIp + ": Got Response!");
                        break;
                    }
                    Console.WriteLine("CORE: " + checkIp + ": No Response...");
                    Thread.Sleep(1000);
                }
                if (!PingHost(checkIp)) {
                    Console.WriteLine("CORE: Response test failed!");
                    Console.WriteLine("CORE: Please enter a valid IP address(Leave empty for repeat): ");
                    string userInput = Console.ReadLine();
                    if (userInput == "") {
                        userInput = checkIp;
                    }
                    checkIp = userInput;
                } else {
                    if (Ping2(checkIp, cl)) {
                        ipValid = true;
                    } else {
                        Console.WriteLine("CORE: CANNOT CONNECT TO CAMERA API!");
                    }
                }
            }
            return checkIp;
        }

        public static bool PingHost(string nameOrAddress) { //check if ip is on the network
            bool pingable = false;
            Ping pinger = null;

            if (nameOrAddress == "") {
                return false;
            }

            try {
                pinger = new Ping();
                PingReply reply = pinger.Send(nameOrAddress, 3);
                pingable = reply.Status == IPStatus.Success;
            } catch (PingException) {
                Console.WriteLine("CORE: Ping Failed!");
                return false;
            } finally {
                if (pinger != null) {
                    pinger.Dispose();
                }
            }
            return pingable;
        }

        public static bool Ping2(string ip, HttpClient pingClient) { //check if ip has some sort of HTTP resonse
            Request pingReq = new Request("PING", "http://" + ip, null, "Checking " + ip + " For HTTP Response...", "CORE: ");

            if (Request.MakeRequest(pingReq, pingClient)){
                return true;
            } else {
                return false;
            }
        }

        private static void Final() { //checks if net settings are applied
            int checkLimit = 5;
            bool checkSettings = false;
            for (int i = 0; i < checkLimit; i++) {
                if (PingHost(newThermalIp)) {
                    checkSettings = true;
                    break;
                } else {
                    Thread.Sleep(1000);
                    Console.WriteLine("Final Checks...(" + i + "/" + checkLimit + ")");
                }
            }
            if (!checkSettings) {
                Console.WriteLine("FAILED APPLYING SOME SETTINGS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.Read();
            } else {
                Console.WriteLine("Done------------------");
                //Console.Read();
            }
        }

        private static string AdjustTime() {
            string time;
            if (Daylight.summerTime == 1) {
                time = DateTime.Now.AddHours(-1).ToString("hh" + ":" + "mm" + ":" + "ss");
            } else {
                time = DateTime.Now.ToString("hh" + ":" + "mm" + ":" + "ss");
            }
            return time;
        }

        public async static Task<string> GetResponseAsync(HttpClient myClient, Request r, int startIndex, bool trim) { //get login token
            using (HttpResponseMessage response = await myClient.GetAsync(r.reqIP)) {
                using (HttpContent content = response.Content) {
                    string returnedMsg = await content.ReadAsStringAsync();
                    string responseToken = returnedMsg;

                    if (trim) { //get the login token
                        responseToken = returnedMsg.Substring(startIndex, 263); //88 if release, 85 sometimes
                    }

                    if (response.IsSuccessStatusCode) {
                        returnedMsg = r.reqCamType + "Success!";
                        if (!trim) {
                            responseToken = returnedMsg;
                        }
                    }
                    Console.WriteLine(returnedMsg);
                    return responseToken;
                }
            }
        }

        public async static Task<bool> GetRequestAsync(HttpClient myClient, Request r) { //GET request
            using (HttpResponseMessage response = await myClient.GetAsync(r.reqIP)) {
                    return CheckResponseAsync(r,response).Result;
            }
        }

        public async static Task<bool> PostRequestAsync(HttpClient myClient, Request r) { //POST request
            using (HttpResponseMessage response = await myClient.PostAsync(r.reqIP, r.reqContent)) {
                return CheckResponseAsync(r, response).Result;
            }
        }

        public async static Task<bool> PutRequestAsync(HttpClient myClient, Request r) { //PUT request
            using (HttpResponseMessage response = await myClient.PutAsync(r.reqIP, r.reqContent)) {
                return CheckResponseAsync(r, response).Result;
            }
        }

        private static async Task<bool> CheckResponseAsync(Request r, HttpResponseMessage response) { //return result
            using (HttpContent responseContent = response.Content) {
                string returnedMsg = await responseContent.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) {
                    returnedMsg = r.reqCamType + "Success!";
                } else {
                    returnedMsg = r.reqCamType + "Failed!!!";
                }
                Console.WriteLine(returnedMsg);
                return response.IsSuccessStatusCode;
            }
        }
    }
}
