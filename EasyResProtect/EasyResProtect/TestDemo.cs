using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyResProtect
{
    class TestDemo
    {

        public TestDemo()
        {
            string dir = @"E:/test";
            string[] files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                ResProtect.DoEncrypt(file, "123123123123fasdjfaldkfajlsdfkaldadfad", Filter); //key32位,多去少补
            }
        }

        /// <summary>
        /// file筛选规则
        /// </summary>
        bool Filter(string fileName)
        {
            return !string.IsNullOrEmpty(fileName);
        }

    }
}
