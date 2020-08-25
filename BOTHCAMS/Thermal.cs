using BOTHCAMS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BOTHCAMS {
    class Thermal {

        static int[] tokenIndex = { 88, 85 };

        //GET
        static string login = "/v1/user/login?username=admin&password=admin";
        static string test = "/v1/Config?configName=General";
        //PUT
        static string lang = "/v1/Config?configName=General";
        static string network = "/v1/Config?configName=Network";
        static string automaintain = "/v1/Config?configName=AutoMaintain";
        static string time = "/v1/Config?configName=Time";
        static string other = "/v1/video/videoEnc/SetTempDataParam?enable=true&fps=25";
        static string restart = "/v1/system/reboot";
        //POST
        static string encode = "/v1/video/videoEnc/AddTitle";
        static string OSD = "/v1/video/videoEnc/SetFormat";

        static StringContent langContent = new StringContent("{\"DeviceName\":\"TPC\",\"DeviceNo\":\"1\",\"Language\":\"english\"}");
        static StringContent netContent = new StringContent("{\"DefaultInterface\":\"eth0\",\"Domain\":\"XX\",\"Hostname\":\"XX12345\"," +
            "\"Card\":[{\"Enable\":true,\"Name\":\"eth0\",\"DhcpEnable\":false,\"DnsAutoGet\":false,\"DefaultDns\":\"8.8.8.8\"," +
            "\"StandbyDns\":\"8.8.8.8\",\"IPAddress\":\"" + Base.newThermalIp + "\",\"Gateway\":\"192.168.1.1\",\"SubnetMask\":\"255.255.255.0\"," +
            "\"MTU\":1500," + "\"PhysicalAddress\":\"90:74:9d:00:1f:4c\"}]}");
        static StringContent maintContent = new StringContent("{\"AutoRebootDay\":2,\"AutoRebootEnable\":false,\"AutoRebootHour\":2," +
            "\"AutoRebootMinute\":0}");
        static StringContent timeContent = new StringContent("{\"TimeZone\":{\"Hour\":0,\"Minute\":0},\"Ntp\":{\"Enable\":true," +
            "\"Address\":\"192.168.1.103\",\"Port\":123,\"UpdatePeriod\":10}}");
        static StringContent encodeContent = new StringContent("{\"Index\":0,\"Enable\":0,\"X\":4190,\"Y\":10,\"Width\":4000,\"Height\":490," +
            "\"FgColor\":16777215,\"BgColor\":0,\"AutoTurn\":1,\"CalendarRef\":0,\"AlignType\":3,\"TitleStr\":\"\"}");
        static StringContent OSDContent = new StringContent("{\"BitRate\":2048,\"Brc\":0,\"Compress\":5,\"EnhanceVBRMode\":0,\"Fps\":25," +
            "\"Enable\":true,\"GOP\":25,\"ImageQuality\":3,\"ImageSize\":5,\"RefMode\":0}");

        public static string networkMsg = "Changing Net Settings...";
        static string loginMsg = "Getting Login Token...";
        static string testMsg = "Testing Login Token...";
        static string langMsg = "Changing Language...";
        static string automaintainMsg = "Disabling Auto Reboot...";
        static string timeMsg = "Adjusting Time Settings...";
        static string otherMsg = "Applying Other Settings...";
        static string restartMsg = "Restarting Camera...";
        static string encodeMsg = "Changing Encoding Settings...";
        static string OSDMsg = "Disabling OSD Setting...";

        static string thermalCam = "THERMAL: ";

        public static async Task<bool> DoThermalRequestsAsync(string curIp) {

            Request loginReq = new Request("LOGIN", curIp + login, null, loginMsg, thermalCam);
            Request testReq = new Request("GET", curIp + test, null, testMsg, thermalCam);
            Request langReq = new Request("PUT", curIp + lang, langContent, langMsg, thermalCam);
            Request netReq = new Request("RESTART", curIp + network, netContent, networkMsg, thermalCam);
            Request maintReq = new Request("PUT", curIp + automaintain, maintContent, automaintainMsg, thermalCam);
            Request timeReq = new Request("PUT", curIp + time, timeContent, timeMsg, thermalCam);
            Request otherReq = new Request("PUT", curIp + other, null, otherMsg, thermalCam);
            Request encodeReq = new Request("POST", curIp + encode, encodeContent, encodeMsg, thermalCam);
            Request OSDReq = new Request("POST", curIp + OSD, OSDContent, OSDMsg, thermalCam);
            Request restartReq = new Request("RESTART", Base.newThermalIp + restart, null, restartMsg, thermalCam);

            Request[] reqList = { loginReq, langReq, otherReq, maintReq, timeReq, encodeReq, OSDReq, netReq };
            //Request[] reqList = { loginReq, ResetThermal(true) }; //enable this and disable the one above to reset settings

            int successCount = 0;

            using (HttpClient client = new HttpClient()) {
                for (int i = 0; i < reqList.Length; i++) {
                    try {
                        if (reqList[i].reqType == "LOGIN") {
                            if (CheckTokens(client, testReq, reqList[i])) {
                                successCount++;
                            } else {
                                return false;
                            }
                        } else {
                            if (Request.MakeRequest(reqList[i], client)) {
                                successCount++;
                            }
                        }
                    } catch {
                        Console.WriteLine(thermalCam + "FAILED GETTING A RESPONSE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    }
                }

                if (successCount == reqList.Length) {
                    Request.MakeRequest(restartReq, client);
                    return true;
                } else {
                    return false;
                }
            }
        }

        private static Request ResetThermal(bool facReset) {
            Request res;
            if (facReset) {
                res = new Request("PUT", "http://" + Base.newThermalIp + "/v1/system/resetFactory", null, "fac reset", null);
            } else {
                res = new Request("PUT", "http://" + Base.newThermalIp + "/v1/system/resetConfig", null, "default reset", null);
            }
            return res;
        }

        private static bool CheckTokens(HttpClient client, Request testReq, Request loginReq) {
            try {
                using (HttpClient testClient = new HttpClient()) {
                    string token = null;
                    string response;
                    for (int i = 0; i < tokenIndex.Length; i++) {
                        Console.WriteLine("THERMAL: Applying Test Token...");
                        token = Request.ResponseRequest(loginReq, client, tokenIndex[i], true);
                        testClient.DefaultRequestHeaders.Add("X-Token", token);
                        response = Request.ResponseRequest(testReq, testClient, tokenIndex[i], false);
                        if (response == thermalCam + "Success!") {
                            break;
                        }
                    }
                    client.DefaultRequestHeaders.Add("X-Token", token);
                    if (token == null) {
                        return false;
                    } else {
                        return true;
                    }

                }
            } catch (System.Net.Http.HttpRequestException h){
                Console.WriteLine("CAUTION: CAMERA IS BUSY AT THE MOMENT! PLEASE WAIT...");
               // Console.WriteLine(h.ToString());
                Console.WriteLine(thermalCam + "Applying Token Failed!!!!!!!!!!!");
                return false;
            }
        }

    }
}

