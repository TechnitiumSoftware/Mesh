/*
Technitium Mesh
Copyright (C) 2018  Shreyas Zare (shreyas@technitium.com)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Threading;

namespace MeshCore
{
    public static class Debug
    {
        #region variables

        static object _lockObj = new object();
        static IDebug _debug;

        #endregion

        #region public static

        public static void SetDebug(IDebug debug)
        {
            if (_debug == null)
                _debug = debug;
        }

        public static void Write(string source, Exception ex)
        {
            if (_debug != null)
                Write(source, ex.ToString());
        }

        public static void Write(string source, string message)
        {
            if (_debug != null)
            {
                Monitor.Enter(_lockObj);
                try
                {
                    _debug.Write("[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "] [" + source + "] " + message + "\r\n");
                }
                catch
                { }
                finally
                {
                    Monitor.Exit(_lockObj);
                }
            }
        }

        #endregion
    }
}
