using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiveClassTest
{
    [Serializable]
    public class Defect
    {
        public int m_nDefectIndex;
        public string m_strInspectionID;
        public int m_nDefectCode;

        public Defect(int Index, string InspectionID, int Code)
        {
            m_nDefectIndex = Index;
            m_strInspectionID = InspectionID;
            m_nDefectCode = Code;
        }
    }
}
