using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class Save : MonoBehaviour
{
    public Text TextName;
    public Text WriteText;

    private int i = 0;

    public void save()
    {

        BattleLog TestLog = new BattleLog();
        TestLog.WriteBattleLog(
            TextName.text,
            DateTime.Now.Year,
            DateTime.Now.Month,
            DateTime.Now.Day);
        
        // NestedClass Test
        TestLog.nested = new BattleLog.Nested();
        TestLog.nested.x = 1;
        TestLog.nested.name = "SHIN JEONG MIN";
        TestLog.nested.nested2 = new BattleLog.Nested.Nested2();
        TestLog.nested.nested2.name2 = "Ohhhhh My God Them";
        TestLog.nested.nested2.nested3 = new BattleLog.Nested.Nested2.Nested3();
        TestLog.nested.nested2.nested3.name3 = "Jesus!!! you God!";
        
        ES3Settings.defaultSettings.path = "Test_DateFile.es3";
        Debug.Log("TestLog : " + TestLog);
        ES3.Save<BattleLog>("TestNestedClass", TestLog, "Test_NestedClass.es3");
    }

    public void load()
    {
        // NestedClass Test
        BattleLog testLog = new BattleLog();
        testLog.nested = new BattleLog.Nested();
        testLog = ES3.Load<BattleLog>("TestNestedClass", "Test_NestedClass.es3");

        Debug.Log("name : " + testLog.nested.name + " x : " + testLog.nested.x);
        Debug.Log("name2 : " + testLog.nested.nested2.name2);
        Debug.Log("name3 : " + testLog.nested.nested2.nested3.name3);
    }
}
