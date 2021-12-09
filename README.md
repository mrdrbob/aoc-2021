# Bob's Advent of Code 2021

So here's the deal. I'm doing Advent of Code 2021. I'm also still doing Advent of Code 2020 (got 4.5 challenges remaining). I was hoping to wrap up 2020 before 2021 started, but here we are. Typically, I've done these in Rust because it's fun and this is a low-pressure opportunity to play around with a language I don't normally get to use. But for a number of reasons, I'm just short on time this year and will just be doing C#. Deal with it.

## Day 1 - Part 1

Day one is usually kind of a warmup exercise. Like, hey, remember code?

This could be written as a one-liner using tuples, but I created a small `Accumulator` record class just to make things clearer. Applied this to an aggregator and BOOM.

`Aggregate` is one of those LINQ features too few people know about (and I was one of those people for quite a long time). Want to write your own sum for "sum" reason? `list.Aggregate(0, (acc, val) => acc + val)`. Want to multiply instead of add? `list.Aggregate(1, (acc, val) => acc * val)`. Lots of stuff you can do in not a lot of code.

## Day 1 - Part 2

This was largely a matter of applying a rolling queue to the values and yielding the sum of the queue when it was full. I wrapped this logic up into an extension method that I could then just drop in the LINQ chain.

## Day 2 - Part 1

This one is mostly a matter of following instructions and maintaining state correctly. Once again, I leaned on my old pal `Aggregate` to do a lot of the work for me. I also leaned on a couple simple record types to make things more clear / explicit. Also, the `with` syntax is a pretty slick way of working with immutable data. A bit overkill here, though.

## Day 2  - Part 1

A relatively small tweak to the logic. I thought I'd maybe have to abandon 32-bit `int`s here if the multiplication got too large, but it ran just fine as-is.

## Day 3 - Part 1

My approach here is to create 12 `BitCount` structs (one for each "column" of bits), then track the number of "on" and "off" bits for each column with those structs. Once I have the counts, I iterate through the columns left-to-right, and for each one where "on" is the most common, I `OR` a bit into an output value in the appropriate place. I added a couple small extension methods / record types to help hopefully make the code slightly more clear.

One weird aspect is that I'm iterating the bits from left to right (or index 0 to 12 in the array), but when setting the bits in the final number, I have to reverse that to be 11 to 0 to match the amount I shift my mask. Thus: `(1u << (BitLength - position - 1))`. Maybe that's obvious, but seems like it could be a common stumbling point.

Also, if you're not familiar with bitwise operations, you may find the Epsilon calculation a bit weird: `~gamma & 0x0FFF`. What I'm doing is flipping all the bits, then masking off only the bits I want (keep in mind I'm using unsigned 32-bit numbers, if I didn't mask the number would be YUGE).

For example:

| value           | representation in binary         | Description                                                                                               |
| --------------- | -------------------------------- | --------------------------------------------------------------------------------------------------------- |
| gamma           | 00000000000000000000000100101001 | An example gamma value                                                                                    |
| ~gamma          | 11111111111111111111111011010110 | Gamma, but inverted                                                                                       |
| 0X0FFF          | 00000000000000000000111111111111 | A mask where only the 12 right-most bits are set                                                          |
| ~gamma & 0X0FFF | 00000000000000000000111011010110 | The mask "applied" to inverted gamma, zeroing out the 20 left-most bits, leaving the 12 right bits intact |

Magic!

## Day 3 - Part 2

The key thing I missed early on when attempting this puzzle is this: when you're calculating if there are more, less, or equal on bits vs. off bits, *only consider the bits in the list that has matched so far*. I was using the number of on/off bits for the entire set, but once you've eliminated items from your set of possible matching numbers, don't consider them as part of your on/off calculations.

Beyond that, it was mostly a matter of iterating through the positions, reducing your list(s) with each iteration. I did this recursively, but in retrospect, there wasn't any particular reason to do it that way. It was just the first thing that came to mind.

## Day 4 - Part 1

There are quite a few ways to represent the board. I chose to represent it as a dictionary of positions mapping to numbers, and a set of already matched numbers. I created a `Position` record struct, but this just as easily could've been a tuple.

Calculating the win condition is a little wonky as written:

```csharp
    public bool AnyWins()
        => Enumerable.Range(0, 5).Any(y => Enumerable.Range(0, 5).All(x => MatchAt(new Position(x, y)))) ||
            Enumerable.Range(0, 5).Any(x => Enumerable.Range(0, 5).All(y => MatchAt(new Position(x, y))));
```

Each side of the `||` checks if either all columns or rows contain any row/column in which every square matches. Break it down going from the inside out: `Enumerable.Range(0, 5).All(x => MatchAt(new Position(x, y))` - For this row, are all boxes a match? `Enumerable.Range(0, 5).Any(y => ... )` - check every row for this. The repeat the whole thing, but for columns.

The only real hiccup I ran into here was the following line. Can you spot the bug?

`var cards = lines.Skip(1).Select(BingoCard.FromString);`

If you said "That will execute the LINQ statement every time you run it and give you a whole new set of Bingo cards which will cause problems if you mutate the ones in the set", you get a gold star! The fix was simple:

`var cards = lines.Skip(1).Select(BingoCard.FromString).ToList();`

Lazy LINQ evaluation strikes again!

## Day 4 - Part 2

Instead of returning the first winning card, I need to return the last. Again, lots of ways to do this and I chose what I *think* was the laziest.

I rewrote my method of finding the first winning card to `yield return` every card that wins (keeping track of previous winning cards so as not to return them a second time). Then I just grab the `.Last()` one.

## Day 5 - Part 1

This one is largely a matter of "calculating" lines, and then tracking how many times any given point is crossed. The tricky part is largely getting all the details right. For example:

```csharp
    var x = Start.X;
    return Utilities.RangeFromTo(Start.Y, End.Y).Select(y => new Position(x, y));
```

It's tempting to use `Enumerable.Range` here, but `Enumerable.Range` accepts a starting number and a *count*. I added a simple utility to create a range from a starting point and an ending point, specifically going from the smaller number to the larger number.

```csharp
    public static IEnumerable<int> RangeFromTo(int start, int end)
    {
        int rangeStart = Math.Min(start, end);
        int rangeEnd = Math.Max(start, end);
        return Enumerable.Range(rangeStart, rangeEnd - rangeStart + 1);
    }
```

## Day 5 - Part 2

The diagonal lines poked at a flaw in my `RangeFromTo` method. With a completely horizontal or vertical line, it doesn't matter which direction you iterate the points. With a diagonal, you need to iterate the X and Y in a consistent direction with respect to the start/end points. So I rewrote the method:

```csharp
        public static IEnumerable<int> RangeFromTo(int start, int end)
        {
            int step = start > end ? -1 : 1;
            int pos = start;
            while (pos != end)
            {
                yield return pos;
                pos += step;
            }

            yield return pos;
        }
```

This will always iterate from `start` to `end`. I then generate points for both the X and Y positions, and then `Zip` them together:

```csharp
    var xRange = Utilities.RangeFromTo(Start.X, End.X);
    var yRange = Utilities.RangeFromTo(Start.Y, End.Y);
    return xRange.Zip(yRange, (x, y) => new Position(x, y));
```

`Zip` is one of those utilities I almost never need in my day-to-day work. You can use it to combine two equal length lists into a single list of both. In this case, I'm combining a list of X values and Y values into a single list of `Position` values.

## Day 6 - Part 1

I took a naive approach to this puzzle, which was to model each individual fish and keep a list of them. This is the most direct way to conceptualize the problem and works just fine for smallish . However, I see part two is going to require running to a much bigger number.  Given that the fish population is growing exponentially… I don't think my solution will work for part 2.

Spoiler: it doesn't. At least, not in any reasonable amount of time/memory.

## Day 6 - Part 2

So the direct approach of modeling individual fish doesn't work. Dealing with a `List<>` of ~1,600,000,000,000 individual fish is not particularly efficient. Another approach is to model the *population* of fish at each *age*.

Take the example input: `3,4,3,1,2`. Instead of saying, "I have one fish at age 3, one at age 4, one at age 3, one at age 1… etc", group fish together by age: "For age 3, I have two fish. For age 4, I have one fish… etc". Then instead of processing individual fishes, you move the entire population of each age to the next progression.

For example, the population of age 4 fish becomes the population of age 3 fish (because "age" is actually counting down to zero -- age is a poor term here, perhaps "time until reproduction" would be better). The age 5 fish then become age 4, and so on.

There are two special cases to deal with:

1. All age zero fish reproduce.
2. All age zero fish reset to age 6.

These can be modeled:

1. Set the age 8 population to the age 0 population. Remember all age 8 fish have moved to age 7. All of age 0 has reproduced, so the age 8 number will now equal the age 0 number at the start of the day.
2. Add the age 0 population (from the start of the day) to the current age 6 population. All the age zero fish get reset to age 6, but they don't *replace* the age 6 population. That population still exists, so we add them to it.

## Day 7 - Part 1

This one is by far my most "golfed" answer. Basically I try every position from the smallest position (of all current crab positions) to the largest. For each position I try, I calculate the delta for each crab to that position (the absolute value of desired pos - crab pos) and sum that up. Then I find the position where that sum is the smallest number.

## Day 7 - Part 2

So the small tweak here is to replace the delta calculation (the absolute value of desired pos - crab pos), with a triangle number calculation (and yes, I had to look that up). I didn't bother to find a nice formula (if one exists) for calculating the values, instead I pre-calculate the necessary possible values in a loop and shove them all into a dictionary for quick look up.

Then I can just use the delta calculation to lookup the final fuel usage for each distance.

## Day 8 - Part 1

Part one is one of those challenges that is largely setting you up for part two. At it's core, part is just parsing the files and counting a few string lengths. Once you put the parsing ceremony aside, it's very nearly a one-liner.

```csharp
var uniqueLengths = new HashSet<int> { 2, 4, 3, 7 };
var uniques = numbers.Select(numbers => numbers.OutputValues.Where(v => uniqueLengths.Contains(v.Length)).Count()).Sum();
```

## Day 8 - Part 2

Part two was quite a bit of fun. For me, the key insight was to deconstruct what I knew about the segments and look for patterns and properties I could exploit. Part 1 gives some big hints in this direction.

For example, let's break down each number, which segments it uses, and the number of segments each uses:

| Number | Segments  | Count | Unique Count? |
| ------ | --------  | ----- | ------------- |
| 0      | `abc_efg` | 6     | No            |
| 1      | `__c__f_` | 2     | Yes - 2       |
| 2      | `a_cde_g` | 5     | No            |
| 3      | `a_cd_fg` | 5     | No            |
| 4      | `_bcd_f_` | 4     | Yes - 4       |
| 5      | `ab_d_fg` | 5     | No            |
| 6      | `ab_defg` | 6     | No            |
| 7      | `a_c__f_` | 3     | Yes - 3       |
| 8      | `abcdefg` | 7     | Yes - 7       |
| 9      | `abcd_fg` | 6     | No            |

So, per part 1, four numbers can be recognized simply by the number of segments lit up. Looking at them closer, we can see that 1 and 7 both share "c" and "f", but only 7 uses "a". So by looking for a pattern with three letters (making it number 7), and looking for the segment that does *not* appear in the two letter pattern (number 1), we can find the segment that corresponds to "a".

That's a good start, but we need more information for some of the other segments. Let's look at how many times each segment is used throughout all ten possible patterns:

| Segment | Frequency | Unique Count? |
| ------- | --------- | ------------- |
| a       | 8         | No            |
| b       | 6         | Yes - 6       |
| c       | 8         | No            |
| d       | 7         | No            |
| e       | 4         | Yes - 4       |
| f       | 9         | Yes - 9       |
| g       | 7         | No            |

Because we have all ten possible patterns, we can look at how frequently each segment occurs in those ten arrangements. For the three segments with a unique frequency -- we can figure those out directly.

* Whatever character occurs 6 times in the ten unique patterns maps to "b".
* Whatever character occurs 4 times is "e". 
* Whatever character occurs 9 times is "f".

There's four mapped. The other letters do not occur a unique number of times in the patterns, but we can look for other differentiators as necessary.

* "d" and "g" will both occur 7 times in the unique patterns, but only "d" will appear in the four pattern. We can uniquely identify the four pattern because it has 4 segments.
* "a" and "c" will both appear 8 times in the unique patterns, but only "c" will appear in the "one" pattern, which we identify as the one with 2 segments.

At this point, we've solved everything except "g", which will be whatever letter remains unsolved.

Now that we've built a mapping of mixed up segments to correct segments, we can apply that, look up each number, and aggregate the result in the final decimal number.

## Day  9 - Part 1

Once again we've got a map of data we need to scan. Once again, I represent the map as a single-dimension array of data.

One small way to make this simpler is to make a function gets values at a point and *handles invalid positions*. Somewhere you have to check the above, below, left and right points. You could put your "point is valid" check for each of those positions, but that's four times the logic and four times the potential for mistakes. I'd suggest you centralize it in the "get value at point" logic.
