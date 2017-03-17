using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 让Distinct函数中的自定义模式使用更便捷。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultEqualityComparer<T> : IEqualityComparer<T>
    {
        public DefaultEqualityComparer(Func<T, T, bool> compareLogic)
        {
            (compareLogic == null).Then(() => { throw new NullReferenceException("compareLogic"); });
            this.CompareLogic = compareLogic;
        }

        protected Func<T, T, bool> CompareLogic;

        public bool Equals(T x, T y)
        {
            return this.CompareLogic(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }



    /// <summary>
    /// 让Distinct函数中的自定义模式使用更便捷。（比较怪异：Except方法内部会先调用GetHashCode，不同在话直接不走自定义比较逻辑，加此扩展，GetHashCode返回999）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("此函数GetHashCode永远返回999，慎用")]
    public class SameHashEqualityComparer<T> : IEqualityComparer<T>
    {
        public SameHashEqualityComparer(Func<T, T, bool> compareLogic)
        {
            (compareLogic == null).Then(() => { throw new NullReferenceException("compareLogic"); });
            this.CompareLogic = compareLogic;
        }

        protected Func<T, T, bool> CompareLogic;

        public bool Equals(T x, T y)
        {
            return this.CompareLogic(x, y);
        }

        public int GetHashCode(T obj)
        {
            return 999;
        }
    }
}
