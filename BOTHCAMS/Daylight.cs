using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BOTHCAMS {
    class Daylight {

        public static int summerTime = 1; //added here because it automatically shifts the time if enabled

        static string network = "/appquery.cgi?btOk=submit&n_t_dhcp=1&n_t_ip=" + Base.newDayIp + "&n_t_sn=255.255.255.0&n_t_gw=192.168.1.1&" +
            "n_t_dns0=8.8.8.8&n_t_dns1=208.67.222.222&n_t_wport=80";
        static string datetime = "/appquery.cgi?btOK=submit&s_s_svw=1&s_s_svr=192.168.1.103&s_s_svp=3600&s_s_svs=20&s_s_svm=2&s_s_tf=0&" +
            "s_s_st=" + summerTime.ToString() + "&s_s_tz=22&s_s_tso=0";
        static string UPNP = "/appquery.cgi?btOK=submit&n_u_id=EO&n_u_name=EO";
        static string outset = "/appispmu.cgi?btOK=submit&i_o_md=4&i_o_rlv=2&i_o_fq=0";
        static string updateDate = "/param_conf.cgi?&action=update&DatenTime.NewServerTime.TimeMode.SetManually.SetManually=yes" +
            "&DatenTime.NewServerTime.TimeMode.SetManually.Date=";
        static string updateTime = "/param_conf.cgi?&action=update&DatenTime.NewServerTime.TimeMode.SetManually.SetManually=yes" +
            "&DatenTime.NewServerTime.TimeMode.SetManually.Time=";

        static string networkMsg = "Applying Network Settings...";
        static string dateMsg = "Applying Datetime Settings...";
        static string UPNPMsg = "Applying UPnP Settings...";
        static string outsetMsg = "Applying Camera Settings...";
        static string updateDateMsg = "Updating Date...";
        static string updateTimeMsg = "Updating Time...";

        static string daylight = "DAYLIGHT: ";

        public static async Task<bool> DoDaylightRequestsAsync(string curIp, string time, string date) {

            Request netReq = new Request("GET", curIp + network, null, networkMsg, daylight);
            Request dateReq = new Request("GET", curIp + datetime, null, dateMsg, daylight);
            Request UPNPReq = new Request("GET", curIp + UPNP, null, UPNPMsg, daylight);
            Request outsetReq = new Request("GET", curIp + outset, null, outsetMsg, daylight);
            Request upDateReq = new Request("GET", curIp + updateDate + date, null, updateDateMsg, daylight);
            Request upTimeReq = new Request("GET", curIp + updateTime + time, null, updateTimeMsg, daylight);

            Request[] reqList = { upDateReq, upTimeReq, netReq, dateReq, UPNPReq, outsetReq };
            int successCount = 0;

            using (HttpClient client = new HttpClient()) {
                for (int i = 0; i < reqList.Length; i++) {
                    Request.MakeRequest(reqList[i], client);
                    successCount++;
                }
            }

            if (successCount == reqList.Length) {
                return true;
            } else {
                return false;
            }
        }

    }
}
