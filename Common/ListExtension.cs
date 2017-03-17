using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Author:FengZiLi
    /// Date:2012.xx.xx
    /// </summary>
    public static class ListExtension
    {

        /// <summary>
        /// Added by fengzili at 2014.01.25 16:38
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="newList"></param>
        /// <returns></returns>
        public static IList<T> AddRange<T>(this IList<T> source, IEnumerable<T> newList)
        {
            if (source == null)
            {
                source = newList.ToList();
                return source;
            }
            newList.Foreach(item => {
                source.Add(item);
            });
            return source;
        }

        public static IList<T> AddRange<T>(this IList<T> source, params T[] newList)
        {
            if (source == null)
            {
                source = newList.ToList();
                return source;
            }
            newList.Foreach(item =>
            {
                source.Add(item);
            });
            return source;
        }

        /// <summary>
        /// 扩展Framework的Distinct方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="equalLogic"></param>
        /// <returns></returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> collection, Func<T, T, bool> equalLogic)
        {
            IEqualityComparer<T> ecpUser = new DefaultEqualityComparer<T>(equalLogic);
            return collection.Distinct<T>(ecpUser);
        }

        /// <summary>
        /// 是否存在某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="existCheck"></param>
        /// <returns></returns>
        public static bool Has<T>(this IEnumerable<T> collection, Func<T, bool> existCheck)
        {
            if (collection.IsNullOrEmpty() || existCheck == null)
            {
                return false;
            }
            foreach (T t in collection)
            {
                if (existCheck(t))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否存在某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="existCheck"></param>
        /// <returns></returns>
        public static bool Contains<T>(this IEnumerable<T> collection, Func<T, bool> existCheck)
        {
            return collection.Has(existCheck);
        }

        /// <summary>
        /// 是否存在某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <param name="compareLogic"></param>
        /// <returns></returns>
        public static bool Contains<T>(this IEnumerable<T> collection, T value, Func<T, T, bool> compareLogic)
        {
            if (collection.IsNullOrEmpty())
            {
                return false;
            }
            IEqualityComparer<T> eqc = new DefaultEqualityComparer<T>(compareLogic);
            return collection.Contains(value, eqc);
        }


        /// <summary>
        /// Foreach扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<T> Foreach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            if (collection.IsNullOrEmpty() || action == null)
            {
                return collection;
            }
            foreach (T t in collection)
            {
                action(t);
            }
            return collection;
        }

        /// <summary>
        /// Foreach扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IEnumerable<TOut> Foreach<TIn, TOut>(this IEnumerable<TIn> collection, Func<TIn, TOut> action)
        {
            if (collection.IsNullOrEmpty() || action == null)
            {
                yield break;
            }
            foreach (TIn t in collection)
            {
                yield return action(t);
            }
        }

        public static int SafeLength(this ICollection collection)
        {
            if (collection.IsNullOrEmpty())
            {
                return 0;
            }
            return collection.Count;
        }

        public static IList<T> MergeValues<T>(this IList<T> collection, IList<T> values, Func<T, T, bool> compareLogic)
        {
            if (collection == null)
            {
                return collection;
            }
            if (values.IsNullOrEmpty())
            {
                return collection;
            }
            foreach (T tvl in values)
            {
                ListExtension.MergeValue<T>(collection, tvl, compareLogic);
            }
            return collection;
        }

        public static IList<T> MergeValues<T>(this IList<T> collection, Func<T, T, bool> compareLogic, params T[] values)
        {
            if (collection == null)
            {
                return collection;
            }
            foreach (T tvl in values)
            {
                ListExtension.MergeValue<T>(collection, tvl, compareLogic);
            }
            return collection;
        }

        public static IList<T> MergeValue<T>(this IList<T> collection, T value, Func<T, T, bool> compareLogic)
        {
            if (compareLogic != null)
            {
                if (collection.Contains<T>(value, new DefaultEqualityComparer<T>(compareLogic)))
                {
                    return collection;
                }
            }
            else
            {
                if (collection.Contains(value))
                {
                    return collection;
                }
            }
            collection.Add(value);
            return collection;
        }

        public static IList<T> MergeValue<T>(this IList<T> collection, T value)
        {
            collection.MergeValue(value, null);
            return collection;
        }

        public static T Previous<T>(this IList<T> collection, T currentObject, T emptyValue, bool throwWhenNotFindCurrentElement = false, bool throwWhenNoPreviousElement = false)
        {
            if (collection.IsNullOrEmpty())
            {
                return emptyValue;
            }
            int iPosition = collection.IndexOf(currentObject);
            if (iPosition == -1)
            {
                if (throwWhenNotFindCurrentElement)
                {
                    throw new KeyNotFoundException("没有找到该元素");
                }
                return emptyValue;
            }
            if (iPosition == 0)
            {
                if (throwWhenNoPreviousElement)
                {
                    throw new KeyNotFoundException("没有找到前置元素");
                }
                return emptyValue;
            }
            return collection[iPosition - 1];
        }

        public static T Next<T>(this IList<T> collection, T currentObject, T emptyValue, bool throwWhenNotFindCurrentElement = false, bool throwWhenNoNextElement = false)
        {
            if (collection.IsNullOrEmpty())
            {
                return emptyValue;
            }
            int iPosition = collection.IndexOf(currentObject);
            if (iPosition == -1)
            {
                if (throwWhenNotFindCurrentElement)
                {
                    throw new KeyNotFoundException("没有找到该元素");
                }
                return emptyValue;
            }
            if ((iPosition + 1) >= collection.Count)
            {
                if (throwWhenNoNextElement)
                {
                    throw new KeyNotFoundException("没有找到后置元素");
                }
                return emptyValue;
            }
            return collection[iPosition + 1];
        }

        public static int IndexOf<T>(this IList<T> collection, T value, Func<T, T, bool> equalLogic)
        {
            if (collection.IsNullOrEmpty() || equalLogic == null)
            {
                return -1;
            }
            int i = 0;
            foreach (T t in collection)
            {
                if (equalLogic(value, t))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        #region 转成DataTable

        /// <summary>
        /// 创建DataTable 把列都设置好
        /// </summary>
        /// <typeparam name="T">与列对象的对象原型</typeparam>
        /// <returns>返回创建的DataTable</returns>
        private static DataTable CreateDataTable<T>()
        {
            Type tEntityType = typeof(T);
            DataTable dtTable = new DataTable(tEntityType.Name);
            PropertyDescriptorCollection pdcProperties = TypeDescriptor.GetProperties(tEntityType);

            foreach (PropertyDescriptor prop in pdcProperties)
            {
                dtTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            return dtTable;
        }

        /// <summary>
        /// 将IEnumerable 转成DataTable(注意复杂属性类型不会自动转换，如list、enum)
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="list">list集合</param>
        /// <returns>DataTable </returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            if (list.IsNullOrEmpty())
            {
                return new DataTable();
            }
            DataTable dtTable = ListExtension.CreateDataTable<T>();
            Type tEntityType = typeof(T);
            PropertyDescriptorCollection pdcProperties = TypeDescriptor.GetProperties(tEntityType);

            foreach (T item in list)
            {
                DataRow drRow = dtTable.NewRow();

                foreach (PropertyDescriptor prop in pdcProperties)
                {
                    drRow[prop.Name] = prop.GetValue(item);
                }

                dtTable.Rows.Add(drRow);
            }

            return dtTable;
        }
        #endregion
    }
}
