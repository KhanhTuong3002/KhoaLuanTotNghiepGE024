using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Question
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Images { get; set; }
    public string setId { get; set; }
    public string Content { get; set; }
    public string Option1 { get; set; }
    public string Option2 { get; set; }
    public string? Option3 { get; set; }
    public string? Option4 { get; set; }
    public string Answer { get; set; }
    public string Description { get; set; }
    public DateTime Published { get; set; }
}
