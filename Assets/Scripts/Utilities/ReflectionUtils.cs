using System;
using System.Linq;
using System.Reflection;

namespace LightControls.Utilities
{
    public static class ReflectionUtils
    {
        public static object GetMemberAtPath(object memberContainer, string path)
        {
            string[] propertyNames = path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (propertyNames.Length > 0)
            {
                //Here we handle special cases
                switch (propertyNames[0])
                {
                    case "Array":
                        int index;
                        bool isSpecialCase = VerifyArraySpecialCase(propertyNames, out index);
                        if (isSpecialCase)
                        {
                            object specialValue = (memberContainer as Array).GetValue(index);

                            if (propertyNames.Length > 2)
                            {
                                string newPath = path.Substring(propertyNames[0].Length + propertyNames[1].Length + 2, path.Length - (propertyNames[0].Length + propertyNames[1].Length) - 2);

                                specialValue = GetMemberAtPath(specialValue, newPath);
                            }

                            return specialValue;
                        }
                        break;
                    default:
                        break;
                }
                
                MemberInfo foundMember = memberContainer
                    .GetType()
                    .FindMembers(
                        memberType: MemberTypes.Field | MemberTypes.Property,
                        bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        filter: NameMatches,
                        filterCriteria: propertyNames[0])
                    .Single();

                object memberValue = foundMember.MemberType == MemberTypes.Field
                    ? (foundMember as FieldInfo).GetValue(memberContainer)
                    : (foundMember as PropertyInfo).GetValue(memberContainer);

                if (propertyNames.Length > 1)
                {
                    string newPath = path.Substring(propertyNames[0].Length + 1, path.Length - propertyNames[0].Length - 1);

                    memberValue = GetMemberAtPath(memberValue, newPath);
                }
                
                return memberValue;
            }
            else
            {
                return null;
            }
        }

        public static MemberType GetMemberAtPath<MemberType>(object memberContainer, string path)
        {
            object memberAtPath = GetMemberAtPath(memberContainer, path);

            if(memberAtPath == null)
            {
                return default(MemberType);
            }
            else if (typeof(MemberType).IsAssignableFrom(memberAtPath.GetType()))
            {
                return (MemberType)memberAtPath;
            }
            else
            {
                throw new Exception("Type given and type found at path did not match");
            }
        }


        public static void SetMemberAtPath(object memberContainer, object setAs, string path)
        {
            string[] propertyNames = path.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (propertyNames.Length > 0)
            {
                //Here we handle special cases
                switch (propertyNames[0])
                {
                    case "Array":
                        int index;
                        bool isSpecialCase = VerifyArraySpecialCase(propertyNames, out index);
                        if (isSpecialCase)
                        {
                            if (propertyNames.Length > 2)
                            {
                                object specialValue = (memberContainer as Array).GetValue(index);

                                string newPath = path.Substring(propertyNames[0].Length + propertyNames[1].Length + 2, path.Length - (propertyNames[0].Length + propertyNames[1].Length) - 2);
                                
                                SetMemberAtPath(specialValue, setAs, newPath);
                            }
                            else
                            {
                                (memberContainer as Array).SetValue(setAs, index);
                            }
                        }
                        break;
                    default:
                        break;
                }

                MemberInfo foundMember = memberContainer
                    .GetType()
                    .FindMembers(
                        memberType: MemberTypes.Field | MemberTypes.Property,
                        bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        filter: NameMatches,
                        filterCriteria: propertyNames[0])
                    .Single();

                object memberValue = foundMember.MemberType == MemberTypes.Field
                    ? (foundMember as FieldInfo).GetValue(memberContainer)
                    : (foundMember as PropertyInfo).GetValue(memberContainer);

                if (propertyNames.Length > 1)
                {
                    string newPath = path.Substring(propertyNames[0].Length + 1, path.Length - propertyNames[0].Length - 1);

                    SetMemberAtPath(memberValue, setAs, newPath);
                }
                else
                {
                    if(foundMember.MemberType == MemberTypes.Field)
                    {
                        (foundMember as FieldInfo).SetValue(memberContainer, setAs);
                    }
                    else
                    {
                        (foundMember as PropertyInfo).SetValue(memberContainer, setAs);
                    }
                }
            }
        }
        
        private static bool NameMatches(MemberInfo memberInfo, object filterCriteria)
        {
            if (typeof(string).IsAssignableFrom(filterCriteria.GetType()))
            {
                string filterName = (string)filterCriteria;

                return memberInfo.Name == filterName;
            }
            else
            {
                throw new Exception("Filter Criteria must be a string or assignable to a string.");
            }
        }

        private static bool VerifyArraySpecialCase(string[] commands, out int index)
        {
            if(commands.Length >= 2)
            {
                string test = commands[1].Substring(5, commands[1].Length - 6);

                bool firstMatches = commands[0] == "Array";
                bool secondMatches = int.TryParse(commands[1].Substring(5, commands[1].Length - 6), out index) && commands[1].StartsWith("data[") && commands[1].EndsWith("]");
                
                return firstMatches && secondMatches;
            }

            index = 0;

            return false;
        }
    }
}