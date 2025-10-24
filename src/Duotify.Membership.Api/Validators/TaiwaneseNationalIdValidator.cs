namespace Duotify.Membership.Api.Validators;

public class TaiwaneseNationalIdValidator
{
    private static readonly string ValidLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly int[] Weights = { 1, 9, 8, 7, 6, 5, 4, 3, 2, 1, 1 };
    private static readonly int[] LetterCodes = 
    {
        10, 11, 12, 13, 14, 15, 16, 17, 34, 18, 19, 20, 21, 22, 35, 23, 24, 25, 26, 27, 28, 29, 32, 30, 31, 33
    };

    public bool IsValid(string nationalId)
    {
        if (string.IsNullOrEmpty(nationalId) || nationalId.Length != 10)
            return false;

        if (!char.IsLetter(nationalId[0]))
            return false;

        if (!nationalId.Substring(1).All(char.IsDigit))
            return false;

        int letterCode = GetLetterCode(nationalId[0]);
        if (letterCode == -1)
            return false;

        int sum = (letterCode / 10) + (letterCode % 10) * 9;

        for (int i = 1; i < 10; i++)
        {
            int digit = int.Parse(nationalId[i].ToString());
            sum += digit * Weights[i];
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        int providedCheckDigit = int.Parse(nationalId[9].ToString());

        return checkDigit == providedCheckDigit;
    }

    private int GetLetterCode(char letter)
    {
        int index = ValidLetters.IndexOf(char.ToUpper(letter));
        if (index == -1)
            return -1;

        return LetterCodes[index];
    }
}
