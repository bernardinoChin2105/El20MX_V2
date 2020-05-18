using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Utils
{
    //<summary>Clase que contiene metodos que ayudan a realizar operacione con las fechas.</summary>
    public class DateUtil
    {
        private static string TIME_ZONE = "Central Standard Time (Mexico)";
        enum Period { Minutes, Hours, Days, Months }


        public static DateTime AddPeriod(DateTime date, double increment, string period)
        {
            DateTime dateResult = date;
            if (period.Equals(Period.Minutes.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return dateResult.AddMinutes(increment);
            }
            if (period.Equals(Period.Hours.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return dateResult.AddHours(increment);
            }
            if (period.Equals(Period.Days.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return dateResult.AddDays(increment);
            }
            if (period.Equals(Period.Months.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return dateResult.AddMonths(Convert.ToInt32(increment));
            }
            return dateResult.AddMinutes(increment);
        }
        public static DateTime AddPeriod(DateTime date, double increment)
        {
            string period = Period.Minutes.ToString();
            return AddPeriod(date, increment, period);

        }
        /// <summary>
        /// Retorna la fecha de ahora para la zona de tiempo especificaa
        /// </summary>
        /// <param name="timeZoneId">Zona de tiempo</param>
        /// <returns>Fecha actual</returns>
        public static DateTime GetDateTimeNow(string timeZoneId)
        {
            var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, myTimeZone);
        }
        /// <summary>
        /// Retorna la fecha de ahora para la configuracion default.
        /// </summary>
        /// <returns>Fecha de ahora para la zona de tiempo a</returns>
        public static DateTime GetDateTimeNow()
        {
            return GetDateTimeNow(TIME_ZONE);
        }
        /// <summary>Convierte la fecha unix en un objeto de tipo DateTime.</summary>
        /// <param name="unixtime">Fecha en formato unix</param>
        /// <returns>Objeto DateTime correspondiente a la fecha unix</returns>
        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return sTime.AddSeconds(unixtime);
        }
        /// <summary>Convierte el objeto DateTime en fecha unix.</summary>
        /// <param name="datetime">Objeto que contiene la fecha que se desea convertir a fecha unix</param>
        /// <returns>Fecha unix correspondiente a la fecha ingresada en el DateTime</returns>
        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(datetime - sTime).TotalSeconds;
        }

        public static bool TryParserObject(object voyagerSailDateObj, out DateTime voyagerSailDate)
        {
            voyagerSailDate = new DateTime();
            if (voyagerSailDateObj is string)
            {
                return (DateTime.TryParse(voyagerSailDateObj.ToString(), out voyagerSailDate));
            }
            if (voyagerSailDateObj is DateTime)
            {
                voyagerSailDate = Convert.ToDateTime(voyagerSailDateObj);
                return true;
            }
            if (voyagerSailDateObj is double)
            {
                double dateAsDouble = Convert.ToDouble(voyagerSailDateObj);
                voyagerSailDate = DateTime.FromOADate(dateAsDouble);
                return true;
            }
            return false;
        }

        public static DateTime GetLastDateTimeOfDay(DateTime endMonth)
        {
            DateTime lastDateTimeOfDay = new DateTime(endMonth.Year, endMonth.Month, endMonth.Day);
            return lastDateTimeOfDay.AddDays(1).AddTicks(-1);
        }

        public static DateTime GetLastDateTimeOfMonth(DateTime endMonth)
        {
            DateTime lastDateTimeOfDay = new DateTime(endMonth.Year, endMonth.Month, DateTime.DaysInMonth(endMonth.Year, endMonth.Month));
            return lastDateTimeOfDay.AddDays(1).AddTicks(-1);
        }
        public static DateTime GetFirstDateTimeOfMonth(DateTime date)
        {
            DateTime firstDateTimeOfMonth = new DateTime(date.Year, date.Month, 1);
            return firstDateTimeOfMonth;
        }
        public static IList<DateTime> GetMonthsBetween(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new Exception("startDate debe ser menor a endDate");
            }
            IList<DateTime> months = new List<DateTime>();
            DateTime currentDate = startDate;
            while (currentDate.Year <= endDate.Year)
            {
                if (currentDate.Month > endDate.Month && currentDate.Year == endDate.Year)
                {
                    break;
                }
                months.Add(currentDate);
                currentDate = currentDate.AddMonths(1);
            }
            return months;
        }

        public static DateTime GetDateTimeOnlyMinutes(DateTime dateTime)
        {
            DateTime newDate = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);
            return newDate;
        }
    }
}