using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LightControls.Utilities
{
    public static class MiscUtils
    {
        public static ArrayType[] ResizeAndFill<ArrayType>(ArrayType[] array, int size)
            where ArrayType : new()
        {
            if (array.Length != size)
            {
                int previousSize = array.Length;

                Array.Resize(ref array, size);
                
                if (size > previousSize)
                {
                    for (int i = 0; i < size - previousSize; i++)
                    {
                        array[previousSize + i] = new ArrayType();
                    }
                }
            }

            return array;
        }
        
        public static ArrayType[] ResizeAndFillWith<ArrayType>(ArrayType[] array, int size, ArrayType defaultValue = default(ArrayType))
        {
            if (array.Length != size)
            {
                int previousSize = array.Length;

                Array.Resize(ref array, size);

                if (size > previousSize)
                {
                    for (int i = 0; i < size - previousSize; i++)
                    {
                        array[previousSize + i] = defaultValue;
                    }
                }
            }

            return array;
        }

        public static ArrayType[] ResizeAndFillWith<ArrayType>(ArrayType[] array, int size, Func<ArrayType> generator)
        {
            if (array.Length != size)
            {
                int previousSize = array.Length;

                Array.Resize(ref array, size);

                if (size > previousSize)
                {
                    for (int i = 0; i < size - previousSize; i++)
                    {
                        array[previousSize + i] = generator.Invoke();
                    }
                }
            }

            return array;
        }
    }
}
