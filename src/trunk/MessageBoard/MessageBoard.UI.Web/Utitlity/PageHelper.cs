using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessageBoard.UI.Web.Utitlity
{
    public class PageHelper
    {

        public static DateTime LocalTime
        {
            get
            {
                const string timeZoneSetting = "W. Central Africa Standard Time";
                var userdate = DateTime.Now;
                try
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneSetting);
                    userdate = TimeZoneInfo.ConvertTime(userdate, timeZone);
                }
                catch (Exception)
                {
                }

                return userdate;
            }
        }
    }
}