/*
 * Genetic Algorithm Test
 * Jarrod Christman
 * 
 * This test is meant to explore genetic algorithms. Essentially it is exploring the math behind evolution
 * (genetic algorithms are part of a class called Evolutionary Algorithms) and how the end result is more efficient (usually by many orders of magnitude)
 * compared to random chance.
 * The utility of this particular implementation is limited, as we're providing the goal, and thus we already know the solution. However, testing an algorithm's ability
 * to find a known solution allows us to judge it's efficiency.
 * 
 * In short, genetic algorithms are a form of an optimization algorithm. You have a goal X that you wish to find an optimal candidate for. The goal X is achieved when a candidate
 * has a specific set of parameters that meet this goal. The permutations possible for these parameters make up a solution space/search space. Speaking in terms of general optimization,
 * the exhaustive search of each possible permutation can take a prohibitively long time. Likewise, a true stochastic method, without heuristics, can be inefficient.
 * Genetic algorithms attempt to solve this by using the math of evolution to optimize a problem more quickly. Each parameter is represented as a 'gene' and a specific
 * value for that parameter is a gene expression. Further, all of the parameters and their values are known as a genome. For the algorithm an individual, in a population of individuals,
 * has a specific genome, which represents a specific solution within the solution search space. There are multiples of these individuals within a larger population of a certain size.
 * For each generation of a population, each individual is ranked by a 'fitness function' that judges how well each individual achieves the optimization goal. For the first generation,
 * the population is usually randomly seeded/generated. From here, two of the top candidates are mated together using some form of genetic mating method, and a new generation/population
 * is generated from this mating result. However, the new generation takes this mating result but has a chance of being mutated themselves. This mutation, based on some mutation rate, 
 * is responsible for allowing the algorithm to explore different regions of the solution space. If there is no mutation, the population becomes static, and a local optimum is found instead.
 * However, if the mutation rate is too high, the algorithm becomes inefficient and starts behaving as more of a random search.
 * 
 * Technically, we can attempt to improve upon this algorithm in more domain specific cases, meaning it depends on the needs of the individual to make these choices. One of the better
 * optimizations of this algorithm is if you can identify invalid permuations. If your specific application doesn't allow for certain permutations, you can exclude these from your population
 * generation, as action called trimming, which will artificially decrease your potential solution space, saving time and computational power from testing permutations that are impossiible.
 * 
 */


using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

// 'ga' for genetic algorithms...
namespace ga
{
    // Class formation
    public partial class Form1 : Form
    {

        // Class Constructor
        public Form1()
        {
            // Required for WinForms GUI
            InitializeComponent();
        }

        // Load event
        // On load set our default settings for population size, mutation rate, and generation count
        private void Form1_Load(object sender, EventArgs e)
        {
            cbpop.SelectedIndex = 2;
            cbmut.SelectedIndex = 2;
            cbgenc.SelectedIndex = 2;
        }

        // On keyboard input for goal string
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Changes all entered goal text to lowercase, mainly because our character list is only lowercase
            textBox1.Text = textBox1.Text.ToLower();
        }

        // Create a random object
        private static Random random = new Random();

        // Our list of characters possible.
        // For genetic algorithms, these are our possible gene expressions
        // Genes in real life can have A, U, G and C. For our algo we're extending this to 27
        // 26 letters and a space.
        private const string charsList = "abcdefghijklmnopqrstuvwxyz ";


        // This function handles the creation of the gene encoding for an individual in our population.
        // The 'genes' of our population are simply the expression of a gene (a letter) at each of the genetic code positions.
        // For our case, the genetic code is the length of the 'goal' string and each gene is a letter/space in that string.
        //
        // For population 0 (the first population), there is no baseString to mutate from, hence a string is entirely randomly
        // generated based on the length provided (length of the goal string).
        // For every population after the first generation there is a baseString (result of mating) for which to mutate from.
        // Here, an individual is created from this baseString with a 'mutateRateRange' chance of each gene being mutated.
        public static string genPopIndividual(int length, string baseString, int mutateRateRange)
        {
            

            // There is no baseString, so this is population 0, randomly generate/seed the first population.
            // Here there still is a chance of our goal string being generated outright, depending on the probability of our
            // goal string being generated purely by random chance. Even with a population of 10,000 the chances are relatively small
            // for strings over 3 characters long.
            if (baseString.Length == 0)
            {
                return new string(Enumerable.Repeat(charsList, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
            else
            {

                // There is a baseString provided. This baseString is provided from the result of mating from a previous generation.
                // Here we convert the baseString to a char array and iterate over it. For each 'gene' we iterate over we have a chance for mutation
                // based on the current mutation rate setting. A mutation too low will result in the baseString not changing much, if at all, 
                // causing the population to be relatively static. A mutation too high will cause the baseString to change too much, resulting in a more
                // random population rather than getting their genes passed from their parents.
                char[] chars = baseString.ToCharArray();
                int charlistlen = charsList.Length;
                int charsLength = chars.Length;

                // Loop through the character positions in the baseString and mutate by chance.
                for (int i = 0; i < charsLength; i++)
                {
                    // A mutation check is performed by seeing if the random number equals 1.
                    // The upper mutation range is changed to fit the mutation chance.
                    // A chance of 100% makes the random range 1,1. A chance of 20% is 1,5. A chance of 50% is 1,2
                    // Any method of selecting a chance for mutation will work however, this is just the implementation here.
                    int MutProbablity = random.Next(1, mutateRateRange);
                    if (MutProbablity == 1)
                    {
                        // We're mutating... here that just means replacing the current character position with a random character.
                        chars[i] = charsList[random.Next(0, charlistlen)];
                    }
                }

                // Return individual.
                return new String(chars);
            }
        }

        // Here we're generating the entire population. This can mean generating multiple individuals of 'population size' and storing them.
        // The idea is to generate a population and judge each individual based on it's 'fitness.' Fitness is simply how well the individual meets the goal.
        // For us, this goal is how well an individual matches our goal string. For real life this 'goal' might be how well a population responds to a change in environment temperature
        // as an example, or how well it hides from a predator.
        // Our analog for judging fitness is a fitness function that rates an individual based on how well the individual meets our goal.
        // To simplify and keep our program somewhat fast, we're generating our population one individual at a time, rating them, and then only storing the running top two results.
        // These results are then mated together at the end of the population generation.
        public void genPopEntire()
        {

            // Set the random number upper range based on the mutation chance.

            /*
            5 % = 0
            10 % = 1
            20 % = 2
            50 % = 3
            */

            int mutateRateRange = 100;
            switch(cbmut.SelectedIndex)
            {
                case 0:
                    mutateRateRange = 20;
                    break;
                case 1:
                    mutateRateRange = 10;
                    break;
                case 2:
                    mutateRateRange = 5;
                    break;
                case 3:
                    mutateRateRange = 2;
                    break;
                default:
                    mutateRateRange = 100;
                    break;
            }


            // Set the population size based on the selected settings.
            // A larger population should allow for fewer generations to reach the goal. However, since genetic algorithms are based on genes and mating toward a goal,
            // there is thus a law of limiting returns to a larger population size. The population size only helps in seeding more potential solutions randomly compared to a smaller population.
            /*
            10 = 0
            100 = 1
            1,000 = 2
            10,000 = 3
            */

            int popSize = 10;
            switch (cbmut.SelectedIndex)
            {
                case 0:
                    popSize = 10;
                    break;
                case 1:
                    popSize = 100;
                    break;
                case 2:
                    popSize = 1000;
                    break;
                case 3:
                    popSize = 10000;
                    break;
                default:
                    popSize = 10;
                    break;
            }

            // Record the top 2 matches and their scores
            // We record the genes (string) of each top match as well as it's fitness score.
            // Recording the fitness score allows us to judge if one individual is better than the existing top results by comparison.
            // A dummy score of 999,999 is recorded to start... unless you're generating a 999,999 long character string the generated results should always be lower.
            String[] topMatches = new String[] {"", "" };
            int[] topMatchesScore = new int[] {999999, 999999 };

            // Cap the generation to our max generation setting
            int genMax = (cbgenc.SelectedIndex + 1) * 100;
            int genStart = 0;

            string genWinnerMixed = "";
            int goalLength = textBox1.Text.Length;

            // Loop through all generations, either until genMax or result is reached.
            while (genStart <= genMax)
            {
            
                // Loop through entire population generation.
                // Each time checking fitness and selecting a running top 2
                int startloop = 1;
                while (startloop <= popSize)
                {

                    // Generate the individual here. For generation 0 (the first), the individual is completely random.
                    // Other generations are mutated from the genome of the mated parents (the top 2 results from the previous generation).
                    string newIndividual = "";
                    newIndividual = genPopIndividual(goalLength, genWinnerMixed, mutateRateRange);

                    // This judges the fitness of the individual in the population.
                    // This fitness varies based on what is the goal. For us, it is the closeness of the individual to our goal string.
                    int currFitness = LevenshteinDistance.Compute(textBox1.Text, newIndividual);

                    // Is the current value better than the previous top two values?
                    if (currFitness < topMatchesScore[0])
                    {
                        // Replace previous top result with new result
                        topMatchesScore[0] = currFitness;
                        topMatches[0] = newIndividual;
                    }
                    else if (currFitness < topMatchesScore[1])
                    {
                        // Replace previous second top result with new result
                        topMatchesScore[1] = currFitness;
                        topMatches[1] = newIndividual;
                    }

                    // increment our population counter.
                    startloop++;
                }

                // If the fitness score is 0, our goal has been reached.
                if(topMatchesScore[0] == 0)
                {
                    genWinnerMixed = topMatches[0];

                    // Result found, clear all buffered values
                    topMatches[0] = "";
                    topMatches[1] = "";
                    topMatchesScore[0] = 999999;
                    topMatchesScore[1] = 999999;

                    break;
                }
                else
                {
                    // If goal hasn't been reached, mate the top two results and start a new generation
                    genWinnerMixed = geneMate(topMatches[0], topMatches[1]);

                    // Clear buffered values for the next generation. The utility of this is probably fairly limited as the next
                    // generation is likely to be superior to any of the buffered results anyways.
                    topMatches[0] = "";
                    topMatches[1] = "";
                    topMatchesScore[0] = 999999;
                    topMatchesScore[1] = 999999;
                }

                // increment our generation counter.
                genStart++;
            }

            // Display the winner, this hopefully is the same as the goal string. However, if the result wasn't found within the allowed generation
            // count, then it is the best match that was found.
            label2.Text = genWinnerMixed;
            // Display generation count used to find the 'winner'
            label4.Text = (genStart + 1).ToString();

            // Show calculation text
            label11.Text = "Chances if random (" + charsList.Length + "^" + goalLength + "):";

            try
            {
                // Here we calculate what the probability is of finding the above result given pure random chance.
                // It is based on k^n, or in this case (unless code is changed) 27^(string length). That is, the chances are the permutations possible from 27 characters of a given length.
                double chancesRandom = Math.Pow(charsList.Length, goalLength);
                label10.Text = "1 in " + (Decimal)chancesRandom;

                // Here we take the generation count * the population size to find that it took X amount of individuals to produce the winning result.
                // We then divide this by the random chance value calculated above. This gives us a percentage of chance of how likely the winning result would have been
                // found only by chance. This is to illustrate the ability of this algorithm to operate much more successfully than random chance.
                label12.Text = ((Decimal)Math.Round(((popSize * (genStart + 1)) / chancesRandom) * 100, 15)).ToString() + "%";
            }
            catch
            {
                label10.Text = "1 in (too large to calculate. That's large!)";
                label12.Text = "0.000000000000001%";
            }


        }

        // For each generation there are top performing individuals. We take them and do a genetic mating.
        // Here the mating is a simple split of on individual in half and merged with the opposing half of the other.
        // Mating can become more complex and be in multiple areas and segments (mating in 3, 4, or 5 parts...etc).
        // Here, the mating is allowing us to take the best of the generation to base the next generation on... but is making the assumption
        // that the very top performing individual still isn't perfect and that by mating with another top performer, the end result has a higher
        // probability of being an even more ideal candidate than the top performer alone.
        private String geneMate(String individualOne, String individualTwo)
        {
            int splitPoint = (int)Math.Ceiling((float)(individualOne.Length / 2));
            String partOne = individualOne.Substring(0, splitPoint);
            String PartTwo = individualTwo.Substring(splitPoint, individualTwo.Length - splitPoint);
            return partOne + PartTwo;
        }

        // When the user hits 'start,' run the code...
        private void button1_Click(object sender, EventArgs e)
        {
            genPopEntire();
        }
    }
}

// Leven distance algorithm (implementation from dotnetperls.com)
// Outputs the number of changes required for one string to match another.
// Ex. Ant -> Aunt = 1
// For us this is an ideal fitness function as it returns back a single number indicative of a candidates performance.
// Since our problem to be solved is simple, the complexity of this fitness function isn't an issue.
// For more complex problems, especially where simulation is needed, the fitness function may have to be applied more efficiently.
// For example, instead of testing every individual in a population, you assign them to groups and randomly test within the group.
static class LevenshteinDistance
{
    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    public static int Compute(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }
}
