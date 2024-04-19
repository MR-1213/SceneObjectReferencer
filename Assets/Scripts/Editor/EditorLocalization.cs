using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization.Settings;
using System;
using UnityEngine.Localization;

public class EditorLocalization : Editor
{
    [SerializeField] UnityEditor.Localization.StringTableCollection m_collection = null;

    public enum TextTableKey
    {
        FindSourceLabel,
        IsIncludeInactiveComponent,
    }

    public void ChangeLanguage(string language)
    {
        // ローカライゼーションする対象のテキストをすべて持ってきて、言語を変更する
        Debug.Log("ChangeLanguage: " + language);
    }

    public void GetTable()
    {
        for (int i = 0; i < m_collection.StringTables.Count; i++)
        {

            IEnumerator<UnityEngine.Localization.Tables.StringTableEntry> values = m_collection.StringTables[i].Values.GetEnumerator();
            while (values.MoveNext())
            {
                string str0 = values.Current.Key;
                //Debug.Log("Key,Value=" + values.Current.Key + "," + values.Current.Value);
            }
        }
    }

    public void GetValue(string key)
    {
        for (int i = 0; i < m_collection.StringTables.Count; i++)
        {
            IEnumerator<UnityEngine.Localization.Tables.StringTableEntry> values = m_collection.StringTables[i].Values.GetEnumerator();
            while (values.MoveNext())
            {
                if (values.Current.Key == key)
                {
                    //Debug.Log("Value: " + values.Current.Value);
                }
            }
        }
    }
}
