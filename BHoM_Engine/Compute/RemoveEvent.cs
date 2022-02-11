/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.oM.Base.Debugging;
using System;
using System.Collections.Generic;
using BH.oM.Base.Attributes;

namespace BH.Engine.Base
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [PreviousVersion("5.1", "BH.Engine.Reflection.Compute.RemoveEvent(BH.oM.Base.Debugging.Event)")]
        public static bool RemoveEvent(Event newEvent)
        {
            lock (Global.DebugLogLock)
            {
                Log log = Query.DebugLog();
                bool success = log.AllEvents.Remove(newEvent);
                success &= log.CurrentEvents.Remove(newEvent);
                return success;
            }
        }

        /***************************************************/

        [PreviousVersion("5.1", "BH.Engine.Reflection.Compute.RemoveEvents(System.Collections.Generic.List<BH.oM.Base.Debugging.Event>)")]
        public static bool RemoveEvents(List<Event> events)
        {
            lock (Global.DebugLogLock)
            {
                Log log = Query.DebugLog();
                bool success = true;
                foreach (Event e in events)
                {
                    success &= log.AllEvents.Remove(e);
                    success &= log.CurrentEvents.Remove(e);
                }

                return success;
            }
        }

        /***************************************************/
    }
}



