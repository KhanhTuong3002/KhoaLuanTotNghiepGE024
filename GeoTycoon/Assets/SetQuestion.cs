using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SetQuestion
{
    public string Id { get; set; }
    public string SetName { get; set; }
    public int QuestionNumber { get; set; }
    public string? UserId { get; set; }
    public List<Question> questions { get; set; }
}
