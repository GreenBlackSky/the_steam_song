﻿using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;


public class DialogueManager : MonoBehaviour {
    static Dictionary<string, Speaker> speakers;

    public static void LoadAllSpeakers(string level_name) {
        if(speakers == null) {
            speakers = new Dictionary<string, Speaker>();
        } else {
            speakers.Clear();
        }
        XmlSerializer nodeSerializer = new XmlSerializer(typeof(DialogueNode));
        Speaker[] foundSpeakers = FindObjectsOfType<Speaker>();
        foreach(Speaker speaker in foundSpeakers) {
            LoadSpeaker(level_name, speaker, nodeSerializer);
            speakers[speaker.speakerUID] = speaker;
        }
    }

    private static void LoadSpeaker(string level_name, Speaker speaker, XmlSerializer nodeSerializer) {
        string path = "Assets/DialoguesData/" + level_name + "/" + speaker.speakerUID + ".xml";        
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNode root = doc.DocumentElement.SelectSingleNode("/conversation");

        speaker.initialNodeID = Int32.Parse(root.SelectSingleNode("initialLineID").InnerText);
        speaker.speakerFullName = root.SelectSingleNode("fullName").InnerText;

        XmlNode xmlLines = root.SelectSingleNode("lines");
        speaker.lines = new List<DialogueNode>();
        foreach(XmlNode lineData in xmlLines.ChildNodes) {
            speaker.lines.Add(nodeSerializer.Deserialize(new XmlNodeReader(lineData)) as DialogueNode);
        }
    }

    public static Speaker GetSpeaker(string name) {
        return speakers[name];
    }
}
