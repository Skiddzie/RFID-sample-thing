using System;
using System.Collections.Generic;
using System.Text;

public class GlobalHelpers
{
    public static string SafeString(object obj)
    {
        string strRet = "";

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                strRet = obj.ToString();
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return strRet;
    }

    public static int SafeInt(object obj)
    {
        int iRet = 0;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                iRet = Convert.ToInt32(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return iRet;
    }

    public static Int64 SafeInt64(object obj)
    {
        Int64 iRet = 0;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                iRet = Convert.ToInt64(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return iRet;
    }

    public static double SafeDouble(object obj)
    {
        double dRet = 0;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                dRet = Convert.ToDouble(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or 0
        }

        // return
        return dRet;
    }

    public static decimal SafeDecimal(object obj)
    {
        decimal dRet = 0;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                dRet = Convert.ToDecimal(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or 0
        }

        // return
        return dRet;
    }

    public static bool SafeBool(object obj)
    {
        bool bRet = false;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                bRet = Convert.ToBoolean(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return bRet;
    }

    public static DateTime SafeDateTime(object obj)
    {
        DateTime dtRet = DateTime.Now;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                dtRet = Convert.ToDateTime(obj.ToString());
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return dtRet;
    }

    public static String SafeDateOnlyString(object obj)
    {
        String strRet = "";
        DateTime dtRet;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                dtRet = Convert.ToDateTime(obj.ToString());
                strRet = Convert.ToString(dtRet.ToShortDateString());
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return strRet;
    }

    public static String SafeTimeOnlyString(object obj)
    {
        String strRet = "";
        DateTime dtRet;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // try to convert.
                dtRet = Convert.ToDateTime(obj.ToString());
                strRet = Convert.ToString(dtRet.ToString("HH:mm"));
            }
        }
        catch
        {
            // unable to convert, or NULL
        }

        // return
        return strRet;
    }

    public static Guid SafeGuid(object obj)
    {
        Guid guidResult = Guid.Empty;

        try
        {
            // make sure it's not null - performance optimization to prevent the Exception if possible (slow)
            if ((obj != null) && (obj != DBNull.Value))
            {
                // convert
                guidResult = new Guid(obj.ToString());
            }
        }
        catch
        {
        }

        // done
        return guidResult;
    }

    public static string UnescapeString(string txt)
    {
        if (string.IsNullOrEmpty(txt)) { return txt; }
        StringBuilder retval = new StringBuilder(txt.Length);
        for (int ix = 0; ix < txt.Length;)
        {
            int jx = txt.IndexOf('\\', ix);
            if (jx < 0 || jx == txt.Length - 1) jx = txt.Length;
            retval.Append(txt, ix, jx - ix);
            if (jx >= txt.Length) break;
            switch (txt[jx + 1])
            {
                case '\\': retval.Append('\\'); break; // Don't escape
                case '\"': retval.Append('\"'); break; // Quote
                default:                                 // Unrecognized, copy as-is
                    retval.Append('\\').Append(txt[jx + 1]); break;
            }
            ix = jx + 2;
        }
        return retval.ToString();
    }

    public static string RemoveLeadingTrailingDoubleQuote(string txt)
    {
        // bounds check
        if (string.IsNullOrEmpty(txt)) { return txt; }

        // decode the results (remove \" and \\)
        int iStartFrom = 0;
        int iLength = txt.Length;
        if (txt.StartsWith("\""))
        {
            // trim the first and last character
            iStartFrom = 1;
            iLength = txt.Length - 2;
        }

        // convert the results string
        string conv = txt.Substring(iStartFrom, iLength);

        // done
        return conv;
    }
}
