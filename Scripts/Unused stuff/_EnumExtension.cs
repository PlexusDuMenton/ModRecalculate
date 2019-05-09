using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlexusUtils
{


    /* NEED TO BE FUTHER MORE TESTED 

    /// <summary>
    /// Use it to extend and fake an enumerator
    /// </summary>
    /// <typeparam name="T">Enumerator you want to fakeout</typeparam>
    class FakeEnum<T>
    {
        public static Dictionary<string, int> EnumList;

        public static List<string> CustomEnumMember;

        /// <summary>
        /// Used to decalre new enum Member in addition to existing one
        /// </summary>
        /// <param name="EnumMember"></param>
        public static void DeclareNewEnumMember(string EnumMember)
        {
            if (!CustomEnumMember.Contains(EnumMember))
            {
                CustomEnumMember.Add(EnumMember);
            }
            else
            {
                throw new Exception("Mod Proc Manager : Trying to declare an existing Member : " + EnumMember);
            }
        }

        static public void BuildEnum()
        {
            int HiggestValue = Enum.GetValues(typeof(T)).Cast<int>().Max();
            foreach (T enumValue in (T[])Enum.GetValues(typeof(T)))
            {
                EnumList.Add(enumValue.ToString(), Convert.ToInt32(enumValue));
            }
            foreach (string CustomEnum in CustomEnumMember)
            {
                HiggestValue++;
                EnumList.Add(CustomEnum, HiggestValue);
            }
        }

        static public int GetValue(string name)
        {
            return EnumList[name];
        }
        static public String GetName(int value)
        {
            foreach (KeyValuePair<string, int> kv in EnumList)
                if (kv.Value == value)
                    return kv.Key;
            return "None";
        }
    }
    */
}
