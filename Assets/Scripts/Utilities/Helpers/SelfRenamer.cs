using UnityEngine;

public class SelfRenamer : MonoBehaviour
{
    // The list of offensive words as a string array
    private string[] offensiveWords = new string[]
    {
        "Cunt", "Motherfucker", "Fuck", "Bitch", "Shit", "bastard", "cock", "twat", "nigger", "faggot",
        "kike", "wanker", "asshole", "piss", "damn", "retard", "idiot", "arse", "arsehole", "bollocks",
        "bloody", "dick", "prick", "penis", "frigging", "slapper", "dork", "nonce", "tits", "moron",
        "cretin", "negro", "coon", "pikey", "gippo", "golliwog", "spastic", "schizo", "paki", "cocksucker",
        "dyke", "anus", "ass-hat", "ass-jabber", "ass-pirate", "assbag", "assbandit", "assbanger", "assbite", "assclown",
        "asscock", "asscracker", "asses", "assface", "assfuck", "assfucker", "assgoblin", "asshat", "asshead", "asshopper",
        "assjacker", "asslick", "asslicker", "assmonkey", "assmunch", "assmuncher", "assnigger", "asspirate", "assshit", "assshole",
        "asssucker", "asswad", "asswipe", "axwound", "goddammit", "dick-head", "dumb-ass", "goddamned", "mother-fucker", "father-fucker",
        "goddamnit", "jackarse", "abbo", "abo", "anal", "analsex", "arab", "argie", "asian", "assassin",
        "assassinate", "assault", "assbagger", "assblaster", "asscowboy", "asshore", "assjockey", "asskiss", "asskisser", "assklown"
    };

    void Awake()
    {
        // Runs early, before Start() on other scripts
        RenameSelf();
    }

    private void RenameSelf()
    {
        // Pick two random words (can be the same)
        int firstNameIndex = Random.Range(0, offensiveWords.Length);
        int lastNameIndex = Random.Range(0, offensiveWords.Length);

        string firstName = offensiveWords[firstNameIndex];
        string lastName = offensiveWords[lastNameIndex];

        // Set this GameObject's name to "First Last"
        gameObject.name = firstName + " " + lastName;

        // Optional: Log for debugging
        Debug.Log($"Renamed self to '{gameObject.name}'");
    }
}