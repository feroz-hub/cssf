/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Collections.Generic;

namespace HCL.CS.SF.DemoClientWpfApp.Components
{
    public static class Mediator
    {
        private static IDictionary<string, List<Action<object>>> actionList =
           new Dictionary<string, List<Action<object>>>();

        public static void Subscribe(string token, Action<object> callback)
        {
            if (!actionList.ContainsKey(token))
            {
                var list = new List<Action<object>>();
                list.Add(callback);
                actionList.Add(token, list);
            }
            else
            {
                bool found = false;
                foreach (var item in actionList[token])
                {
                    if (item.Method.ToString() == callback.Method.ToString())
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    actionList[token].Add(callback);
                }
            }
        }

        public static void Unsubscribe(string token, Action<object> callback)
        {
            if (actionList.ContainsKey(token))
            {
                actionList[token].Remove(callback);
            }
        }

        public static void Notify(string token, object args = null)
        {
            if (actionList.ContainsKey(token))
            {
                foreach (var callback in actionList[token])
                {
                    callback(args);
                }
            }
        }
    }
}


