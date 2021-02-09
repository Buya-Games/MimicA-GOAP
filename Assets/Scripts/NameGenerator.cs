using UnityEngine;

public static class NameGenerator
{
    public static string CreateRandomName(){
        string[] vowels = new string[] {"a","e","i","o","u"};
        string[] consonants = new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z",
            "ch","sh","th"};
        string rando = "";
        //bool vowel = false;
        int nameLength = Random.Range(3,6);
        int vowelChance = Random.Range(1,10);
        for (int i = 0;i<nameLength;i++){
            int character;
            if (vowelChance > 6){
                vowelChance = 0;
                character = Random.Range(0,vowels.Length);
                rando+=vowels[character];   
            } else {
                character = Random.Range(0,consonants.Length);
                rando+=consonants[character];
            }
            vowelChance += Random.Range(2,7);
        }

        //adding last vowel cuz i like names to end w/ vowel
        int lastOne = Random.Range(0,vowels.Length);
        rando+=vowels[lastOne];

        char firstLetter = char.ToUpper(rando[0]);
        rando = firstLetter + rando.Substring(1);
        // for (int i = 0;i<nameLength;i++){
        //     if (vowel){
        //         vowel = false;
        //         int character = Random.Range(0,vowels.Length);
        //         rando+=vowels[character];
        //     } else {
        //         vowel = true;
        //         int character = Random.Range(0,consonants.Length);
        //         rando+=consonants[character];
        //     }
        // }
        return rando;
    }
}
