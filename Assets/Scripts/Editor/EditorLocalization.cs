using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization.Settings;

public class EditorLocalization : Editor
{
    [SerializeField] UnityEditor.Localization.StringTableCollection m_collection = null;

    public void GetTable()
    {
        for (int i = 0; i < m_collection.StringTables.Count; i++)
        {

            IEnumerator<UnityEngine.Localization.Tables.StringTableEntry> values = m_collection.StringTables[i].Values.GetEnumerator();
            while (values.MoveNext())
            {
                string str0 = values.Current.Key;
                Debug.Log("Key,Value=" + values.Current.Key + "," + values.Current.Value);
            }
        }
    }
}
