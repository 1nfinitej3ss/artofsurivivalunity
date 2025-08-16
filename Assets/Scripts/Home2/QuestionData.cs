using UnityEngine;
using System;

[Serializable]
public class QuestionData
{
    public string questionId;
    public int? maxOccurrences;  // null = unlimited, 1 = once only, 2 = twice max
    public bool hasYearLimit;    // For former "Blue" type questions
    public string text;
    public string scene;
    public OptionData[] options;
    public int minYear;
    public int maxYear;
    public bool isMonthlyEffect;
    public string questionBackgroundKey;
    public string[] requirements;  // For any special requirements
}

[Serializable]
public class OptionData
{
    public string id;
    public string text;
    public bool isGameEnding;
    public string optionsBackgroundKey;
    public EffectData effects;
    public bool isMonthlyEffect;  // Added for monthly effects
    public EffectData monthlyEffects;  // Added for monthly effects
    public PossibleResult[] possibleResults;  // Added for random outcomes
    public RandomResult[] randomResults;  // Added for backward compatibility with old JSON
    public string resultText;  // Added for fixed result text
}

[Serializable]
public class RandomResult
{
    public string resultText;
    public EffectData effects;
}

[Serializable]
public class EffectData
{
    public int money;
    public int career;
    public int energy;
    public int creativity;
    public int time;
}

[Serializable]
public class PossibleResult
{
    public string text;
    public EffectData effects;
} 