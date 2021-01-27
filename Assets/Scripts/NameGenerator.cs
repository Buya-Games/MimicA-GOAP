using UnityEngine;

public static class NameGenerator
{
    public static string CreateRandomName(){
        string[] vowels = new string[] {"a","e","i","o","u"};
        string[] consonants = new string[] {"b","c","d","f","g","h","j","k","l","m","n","p","q","r","s","t","v","w","x","y","z"};
        string rando = "";
        bool vowel = false;
        int nameLength = Random.Range(3,7);
        for (int i = 0;i<nameLength;i++){
            if (vowel){
                vowel = false;
                int character = Random.Range(0,vowels.Length);
                rando+=vowels[character];
            } else {
                vowel = true;
                int character = Random.Range(0,consonants.Length);
                rando+=consonants[character];
            }
        }
        return rando;
    }
}