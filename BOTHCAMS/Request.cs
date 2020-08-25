using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BOTHCAMS {
    class Request {

        public string reqType;
        public string reqIP;
        public StringContent reqContent;
        public string reqMsg;
        public string reqCamType;

        public Request(string rType, string rIP, StringContent rContent, string rMsg, string rCamType) {
            reqType = rType;
            reqIP = rIP;
            reqContent = rContent;
            reqCamType = rCamType;
            reqMsg = rCamType + rMsg;
        }

        public static bool MakeRequest(Request req, HttpClient myClient) {
            Console.WriteLine(req.reqMsg);
            try {
                switch (req.reqType) {
                    case "PUT":
                        return Base.PutRequestAsync(myClient, req).Result;
                    case "GET":
                        return Base.GetRequestAsync(myClient, req).Result;
                    case "PING":
                        return Base.GetRequestAsync(myClient, req).Result;
                    case "POST":
                        return Base.PostRequestAsync(myClient, req).Result;
                    case "RESTART":
                        Base.PutRequestAsync(myClient, req);
                        return true;
                }
            } catch (AggregateException e){
                if (req.reqType == "PING") {
                    Console.WriteLine("CAUTION: CAMERA MAY BE STILL TURNING ON");
                    Thread.Sleep(1000);
                }
                if (req.reqMsg == req.reqCamType + Thermal.networkMsg) {
                    if (Base.PingHost(Base.defaultThermalIp)) {
                        return true;
                    }
                } else {
                    Console.WriteLine(req.reqCamType + "FAILED MAKING REQUEST!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                    return false;
                }
            }
            return false;
        }

        public static string ResponseRequest(Request req, HttpClient loginClient, int sIndex, bool trim) {
            Console.WriteLine(req.reqMsg);
            return Base.GetResponseAsync(loginClient, req, sIndex, trim).Result;
        }

    }
}
