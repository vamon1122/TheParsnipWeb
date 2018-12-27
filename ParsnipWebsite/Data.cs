﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CookieApi;

namespace TheParsnipWeb
{
    public static class Data
    {
        public static string deviceType { get { return Cookie.Read("deviceType"); } }
        public static string deviceLatitude { get { return Cookie.Read("deviceLatitude"); } }
        public static string deviceLongitude { get { return Cookie.Read("deviceLongitude"); } }
    }
}