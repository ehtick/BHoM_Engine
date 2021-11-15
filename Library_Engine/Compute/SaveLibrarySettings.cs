﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;
using BH.oM.Data.Library;
using System.IO;

namespace BH.Engine.Library
{
    public static partial class Compute
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Updates the LibrarySetting used for extraction of datasets to the one provided and stores it in the settings folder.")]
        [Input("settings", "The LibrarySettings object to store on the system.")]
        [Input("replacePreexisting", "If true, any preexisting settings file will be replaced. If false, LibrarySettings will only be updated if no settings already exists.")]
        [Input("refreshLibraries", "If true, all loaded libraries will be refreshed and reloaded, making use of the provided LibrarySettings object.")]
        [Output("sucess", "Returns true of the settings was successfully updated.")]
        public static bool SaveLibrarySettings(LibrarySettings settings, bool replacePreexisting = false, bool refreshLibraries = true)
        {
            if (settings == null)
            {
                Engine.Reflection.Compute.RecordError("Settings object is null and can not be stored.");
                return false;
            }

            if (settings.UserLibraryPaths.Any(x => !x.IsValidUserPath()))
                return false;

            if (File.Exists(Query.m_settingsPath))
            {
                if (!replacePreexisting)
                {
                    Engine.Reflection.Compute.RecordError($"A LibrarySettings object already exists. To replace it, toggle replacePreexisting to true. \nTo add a new path to the old set of Paths, try using the {nameof(AddUserPath)} method.");
                    return false;
                }
                else
                {
                    File.Delete(Query.m_settingsPath);
                }
            }

            if (!Directory.Exists(Path.GetDirectoryName(Query.m_settingsPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(Query.m_settingsPath));

            File.WriteAllText(Query.m_settingsPath, Engine.Serialiser.Convert.ToJson(settings));

            if (refreshLibraries)
                Query.RefreshLibraries();

            return true;
        }

        /***************************************************/
    }
}

