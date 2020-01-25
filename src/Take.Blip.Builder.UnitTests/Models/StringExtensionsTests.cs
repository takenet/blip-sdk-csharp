using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class StringExtensionsTests
    {
        /// <summary>
        /// Test cases get from HackerRank: https://www.hackerrank.com/contests/cse-830-homework-3/challenges/edit-distance
        /// </summary>
        [Theory]
        [InlineData("abc", "abc", 0)]
        [InlineData("abc", "", 3)]
        [InlineData("", "abcd", 4)]
        [InlineData("abcde", "abde", 1)]
        [InlineData("abde", "abcde", 1)]
        [InlineData("abcde", "abxde", 1)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaXaaaaaaaaaaaaaaaaaaaaaaaaaaa", "aaaaaaXaaaaaaaaaaaaaaaaXaaaaaaaaaaaaaaaaaaaaXaaaaaaX", 3)]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaabccccccbaaaaaaaaaaaaaaaaaaaa", "aaaaaaaaaaaaaaaaaaaaaaaaabccccccbaaaaaaaaaaaaaaaaaa", 4)]
        [InlineData("abc*efghijklm---nopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLM+++NOPQRSTU*WXYZ", 8)]
        [InlineData("CGICNWFJZOWBDEVORLYOOUHSIPOICMSOQIUBGSDIROYOMJJSLUPVRBNOOPAHMPNGQXHCPSLEYZEYSDGF", "TBYHUADAJRXTDDKWMPYKNQFMGBOXJGNLOGIKTLKVUKREEEVZMTCJVUZSHNRZKGRQOXMBZYGBNDFHDVLM", 74)]
        [InlineData("NTBFKWGUYSLYRMMPSXNYXAZNZFJIPJDMNPZNLPEEYEONABFCZEZYVGCJBFMGWKQPTTGJDLKKQYJZYFSL", "PEDWJMTVXVGGCLSTOOQEACZJNOVUYXPQGIRAPHFWAESSZKHKGKUEEGVWZXVFJWLQBUBOJMHLEHGKWYTN", 70)]
        [InlineData("RPXZTOSEPHWYBYSOOAKMOOJRSSFHENHDVHOIKVNXMFAEXXTBNPNFPTFGNUPLKDWRSUQYWAUVMNVYJZQL", "MFKSTCDHEPTSMYNDRSNJZOULCCIUXZDCGQZRSRYEODXKNBGBBKSPPCQHJYUSCKMJZBPUBLLXPRCQJYRV", 72)]
        public void CalculateLevenshteinDistance_UnitTest(string s, string t, long expected)
        {
            Assert.Equal(s.CalculateLevenshteinDistance(t), expected);
        }
    }
}
