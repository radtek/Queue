using System;
using System.Reflection;
using System.Collections;

namespace Engine.Cloud.Core.Utils
{
    public abstract class ClonableObject : ICloneable
    {
        public object Clone()
        {
            object newObject = Activator.CreateInstance(this.GetType());
            try
            {
                FieldInfo[] fields = newObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                int i = 0;

                foreach (FieldInfo fi in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    Type ICloneType = fi.FieldType.GetInterface("ICloneable", true);
                    object value = (ICloneType != null) 
                        ? ((ICloneable)fi.GetValue(this) == null) ? null : ((ICloneable)fi.GetValue(this)).Clone() 
                        : fi.GetValue(this);
                    
                    if(value != null)
                        fields[i].SetValue(newObject, value);

                    Type IEnumerableType = fi.FieldType.GetInterface("IEnumerable", true);
                    if (IEnumerableType != null)
                    {
                        IEnumerable IEnum = (IEnumerable)fi.GetValue(this);
                        Type IListType = fields[i].FieldType.GetInterface("IList", true);
                        Type IDicType = fields[i].FieldType.GetInterface("IDictionary", true);

                        int j = 0;
                        if (IListType != null)
                        {
                            IList list = (IList)fields[i].GetValue(newObject);

                            foreach (object obj in IEnum)
                            {
                                ICloneType = obj.GetType().GetInterface("ICloneable", true);

                                if (ICloneType != null)
                                {
                                    ICloneable clone = (ICloneable)obj;
                                    list[j] = clone.Clone();
                                }
                                j++;
                            }
                        }
                        else if (IDicType != null)
                        {
                            //Getting the dictionary interface.
                            IDictionary dic = (IDictionary)fields[i].
                                                GetValue(newObject);
                            j = 0;

                            foreach (DictionaryEntry de in IEnum)
                            {
                                ICloneType = de.Value.GetType().GetInterface("ICloneable", true);

                                if (ICloneType != null)
                                {
                                    ICloneable clone = (ICloneable)de.Value;
                                    dic[de.Key] = clone.Clone();
                                }
                                j++;
                            }
                        }
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {

            }
            return newObject;
        }
    }
}
