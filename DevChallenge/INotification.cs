﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DevChallenge
{
    public interface INotification
    {
        XElement Message { get; }
        XElement Serialized { get; }
    }
}