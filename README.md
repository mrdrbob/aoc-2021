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

I took a naive approach to this puzzle, which was to model each individual fish and keep a list of them. This is the most direct way to conceptualize the problem and works just fine for smallish . However, I see part two is going to require running to a much bigger number.  Given that the fish population is growing exponentially??? I don't think my solution will work for part 2.

Spoiler: it doesn't. At least, not in any reasonable amount of time/memory.

## Day 6 - Part 2

So the direct approach of modeling individual fish doesn't work. Dealing with a `List<>` of ~1,600,000,000,000 individual fish is not particularly efficient. Another approach is to model the *population* of fish at each *age*.

Take the example input: `3,4,3,1,2`. Instead of saying, "I have one fish at age 3, one at age 4, one at age 3, one at age 1??? etc", group fish together by age: "For age 3, I have two fish. For age 4, I have one fish??? etc". Then instead of processing individual fishes, you move the entire population of each age to the next progression.

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

## Day 9 - Part 2

So part 2 throws a minor wrench in the works, where you need to figure out how large each "basin" is. Conceptually, you "fan out" from the center point, then fan out from each of those points, then from each of *those* points. The pattern you're seeing is recursion, but I decided not to go that route.

Instead, I'm using a `Stack<>` to give me recursion-like behavior. Here's the breakdown of how it works:

1. Pre-fill the stack with the low point to start at.
2. Loop the following for as long as there are any points remaining in the stack:
   1. Pop a point off the stack. If I've already processed this point, bail out and loop.
   2. Mark this point as visited (keep a list of visited points in a `HashSet`).
   3. If there is no value at this point (the x, y refer to a point outside the map), bail out and loop.
   4. If the point is a 9, bail out and loop. (We don't want to add anything to the total or queue up any surrounding points)
   5. Otherwise, the point is valid and < 9, so add one to the total.
   6. Push the four surrounding points to the stack. (To be more efficient, one could check here if the points have already been visited *before* adding to the stack. I didn't; I'm lazy.)

Again, this could all probably be a recursive function. In theory, it's possible you could hit a stack overflow if the function recursed too much, but that would be a **lot** of recursion. I did not try this route with this question, though. With a manual stack like above, your "recursion" is pretty much only limited by the amount of available memory.

## Day 10 - Part 1

This is a pretty classic stack problem. The simple version is to iterate through all the characters. If it's an "opening" character, push it on the stack. If it's a closing character, pop whatever's on the stack off. If it's what you expect, keep going. If it's the wrong character, stop and return the score of the wrong character. Then sum that up.

The instructions say to ignore incomplete, but otherwise valid lines. This is implicitly done by just not checking if the stack is empty at the end of a line.

## Day 10 - Part 2

Once again, a lot of little details to get right. I started by modifying my part 1 solution to return both the score *and* the remaining stack. Then for every line that does not have a score (and therefore no invalid characters), I can look at the remaining stack to calculate the part 2 score. That's a matter of popping the stack until it's empty, and for each character looking for the expected closing tag, then multiplying/adding.

A word of caution: the numbers get, like, really big. `int` won't cut it here.

## Day 11 - Part 1

For me, the solution for this one is to carefully separate steps, carefully track which octopuses have flashed, iterating as long as it takes to process all the octopuses that have flashed.

I decided to go with a mutable map this, and write some helper extension methods for mutating map. With those, I can break down the process as such:

1. Add 1 to every octopus.
2. Pull the position for every octopus that is ready to flash (has an energy greater than 9). Add the count to the total number of flashes for this step.
3. Set all of those positions' energy levels to null (so octopuses who flashed don't get more energy -- they should all be 0 at the end of the step)
4. Get all the surrounding positions for every octopus that flashed.
5. Add 1 to every position (that isn't null) surrounding an octopus that flashed.
6. Look for any new octopuses that having reached energy > 9. If there are any, repeat from #2.
7. Otherwise, all flashing is done this round. Set all the null energy levels to 0.
8. Return the total number of flashes for this step.

Execute the above 100 times, summing up the totals.

## Day 11 - Part 2

Part two is taking the solution from part one, and running it until a single step produces 100 flashes (every octopus flashed in a single step). Because the solution from part one returns the number of flashes per step, this modification is pretty trivial.

## Day 12 - Part 1

Today's challenge is a problem of visiting nodes. So I took the recursion-lite approach here, where I use stack to keep track of paths I need to visit.

1. Push the start cave on the stack. (Our starting "path" is just the starting cave.)
2. Loop the following for as long as there is something in the stack:
   1. Pop a path off the stack.
   2. Check the last cave at the end of the current path. Is it "end"? Yield that one.
   3. For each cave that is connected to this one, if it's a big cave, or a small cave that doesn't already appear in this path, then push a new path ending in that connected cave.
   4. LOOP

Then we count up all the yielded caves.

## Day 12 - Part 2

Falling behind a bit here both on completing puzzles, and on writing up puzzles. For part two, I chose to tackle it by first figuring out all the applicable lowercase cave names (excluding `start`). Then for each lowercase name, I follow most of the logic from part one, except that I allow that lowercase cave to be visited at most twice. For me, there were a few keys to getting this one done:

1. The "start" node can only ever be visited once, so I had to add logic to exclude that from the possible nodes to visit twice.
2. I found it helpful to break the logic out for the special case on a separate line. This just made it easier to reason about and get right.
3. I had been just using a simple "contains" against the current path, but for the special case, I had to check the count instead.

For things like this, where the logic starts to feel complicated, the way to make it easier to read and simpler to comprehend is to break it into small chunks, and give each chunk a sensible name. In some cases, that means breaking a long string of inline logic or calculations in a series of well-named variables. Or chunks of instructions into well-named methods/functions.

Now, in my case, `isSpecialCase` is far from well-named, but it is at least a separate chunk that can be reasoned independently of all other pieces of logic related to cave iteration.

## Day 13 - Part 1

I've found myself using 2d grid-like structures in several of these puzzles, so I went ahead and made it official and moved the map structure from Day 11 into it's own, generic thing. Now I can just add extension methods as necessary for various bits of logic.

This one was mostly about getting the `FoldVertical` and `FoldHorizontal` logic correct. There are more elegant ways to do this than what I've done, but I'm good with what I've got.

To do the folding, I create a new Map at the appropriate size for the fold. Then I iterate all the points on the side that is being folded. I calculate where the transposed point would land. Then if either the transposed point is set, or the original point at the transposed position is set, I set that point in the new map.

## Day 13 - Part 2

So in Part 1, all of the folding you'll be doing is on the *halfway line.* When I wrote my Part 1 solution, I inadvertently assumed that would always be the case and used the width/height to calculate the transposed values. But for Part 2, that assumption falls apart. I had to tweak my transposition calculation:

From: `var dest = new Position(map.Width - pos.X - 1, pos.Y);`
To: `var dest = new Position(x - (pos.X - x), pos.Y);`

Now instead of reflecting on the halfway point, I'm reflecting on the axis (`x` in the above example) instead.

## Day 14 - Part 1

Sometimes when you do these you just know the next part is going to be "Good job, hotshot, now run it for twice as long as watch as you computer melts because you did the naive thing!" -- which is exactly what I did. I represent the template just as a list of characters. The polymerization rules are just character pairs mapping to a resulting insertion character.

About the only "clever" thing I did here was to iterate through the list backwards, so I don't have to deal with the fact that my index changes as I insert new characters. But the whole "insert a character at this index" thing is horribly inefficient. It's a small solution, easy to reason about, but it 100% won't work if the next part is "run it longer".

## Day 14 - Part 2

As expected, part 2 was just "do the thing a whole bunch more," which did not work. (Technically, given enough time and memory, I guess it would *eventually* have worked???possibly not before the heat death of universe, but eventually.) So similar to the exponential fish population growth of day 6, we hae to think about modeling this from a different angle.

Rather than a string of characters, think about the problem as a collection of character pairs with a corresponding count. The rules then describe adding to pair counts, rather than inserting characters in random places.

For example, `NNCB` becomes:

| Pair | Count |
| ---- | ----- |
| NN   | 1     |
| NC   | 1     |
| CB   | 1     |

And the first rule like `NN -> C` no longer creates `NCN`, but rather it's corresponding pairs: `NC` and `CN`. If we apply that rule, we get:

| Pair | Count |
| ---- | ----- |
| NN   | 0     |
| NC   | 2     |
| CN   | 1     |
| CB   | 1     |

Notice we lose `NN` (it matched a rule, so we remove that value and add the corresponding values). We carry over the values that did not match a rule (`NC` and `CB`).  And add the values for each insertion rule (`NC` and `CN`). `NC` comes through twice, once because it was carried over, and once as a result of the rule. We some those counts together to get 2.

In practice, it's slightly easier because the rulesets we're given have every possible combination of letters, so you don't actually have to handle the "I don't have a rule for this" case.

The final trick is summing up the character frequency. You can just sum up the first and second character of each pair; each letter is part of a *pair*, so you'll end up with double the value. I would around this by just counting the frequency of the second letter of each pair. That will leave out the first letter of the template, right? Well, *that* letter will never change. It will always be whatever it is, because all insertions happen after it. So I just keep track of what that letter was, and give it a count of one before I start summing up the others.

Finally, I just assumed at a 32 bit signed integer wasn't going to be big enough, so I updating everything to `uint`.

## Day 15 - Part 1

Path finding is not my forte.

I made an attempt with doing absolutely no research, and came up with an algorithm that could reasonably quickly find *a* short path. Just not, *the shortest* path. When I adjusted it to find *the shortest* path, it basically took forever to run.

But after some learning about path-finding algorithms, I decided to give Dijkstra's algorithm a try. I'd write up an explanation, but honestly, [this video](https://youtu.be/EFg3u_E6eHU) would do a much better job explaining than I could in text.

So beyond saying "I used Dijkstra's algorithm," there's not much to say here. My implementation is not particularly efficient. I don't use a priority queue. I don't keep separate sets of explored vs unexplored nodes. So the solution takes ten whole seconds to run on my machine. I'm guessing for part two, I'll need to make some performance improvements.

**Update**: The dude could not abide a 10 second run time. I swapped out my lazy "one list to rule them all" and swapped in two separate dictionaries to have fast look up for explored and unexplored nodes, plus a priority queue for which position to explore next. Rather than adjust priorities in the priority queue, I just queue up the same position again with a smaller risk. When the duplicate position comes up in the queue, because a shorter path has already been found, that position will be in the explored nodes dictionary, so I just pop it off the stack and ignore it if it's already been explored. Seems to work well enough. Runtime went from around 10 seconds to around 200 ms in debug mode.

## Day 15 - Part 2

The only real change I had to make was to make the map larger. I considered writing some utility methods to "stamp" the small map out into a large map, making the value changes as I went.

But then I thought: it's a simple pattern, I could generalize that into a wrapper and make a *virtual* map. So I generalized my `Map<>` structure into an `IMap<>` interface, updated my extension methods to expect `IMap<>` instead, and then wrote a wrapper that tracks the stamp data, then dynamically calculates the values for all the other possible positions.

Beyond that, the improved take on Dijkstra's algorithm from part 1 worked quite well. Around 600 ms in debug mode.

## Day 16 - Part 1

Fundamentally this is a parsing challenge. Now, it probably would have been faster to just hand-write a simple parser (especially given some of the more dynamic rules), but I wrote a [a little parsing library](https://github.com/mrdrbob/parsing), and I couldn't resist using it for this.

For the most part it worked well. I didn't have anything in the base library that could handle parsing rules that depended on parsed content, but it wasn't too difficult to hack-in (see the `SubparseByLength` and `SubparseByTimes` rules).

## Day 16 - Part 2

The hardest part of Part 2 was my dumb assumption that `uint` should be big enough for anyone. I had to refactor some stuff to use `ulong` instead.

Otherwise, the new C# `switch` pattern matching works great here, where I can match types, values, and use expressions for cases.

## Day 17 - Part 1

Oof. I won't lie. My solution here is *ugly*.

First, for part 1 at least, I'm completely ignoring the X access. I run step-by-step simulations trying different starting Y velocities until I find the first starting velocity that works. That should be the shortest y value that lands in the target.

THEN I continue upping the starting velocity until it overshoots the target. That does not actually produce the highest y position, just the first one that overshoots.

THEN I take whatever my velocity is and multiply by five. Then that's how many more times I will bump the velocity and attempt the simulation. I keep the highest successful run from these attempts. Five is a totally arbitrary number, but seemed large enough to come up with the correct result.

This is like calculating pi by throwing hotdogs. Sure, it gets approximately the correct answer, but it's messy and dumb looking.

## Day 17 - Part 2

Part 1 was so ugly I mostly ignored it writing part 2. This time I made a few assumptions and tweaked how I decided what velocities I would try. Then I brute-forced through all the possibilities I thought were realistic.

For X velocities, because the target is always right of 0,0, I start at 1. I try velocities up to the target's farthest vertical edge. If the velocity is more than that, then it will overshoot every time.

For y velocities, it's a little trickier. For my starting velocity, I picked an arbitrary starting point of the most negative horizontal edge multiplied by five. The end velocity is the farthest horizontal edge (inverted, because that's how the y access works), on the same principal as the highest x velocities.

Then I just try every possible combination. I'm sure there are more clever ways to do this, but I'm already several days behind. It works. It's gross. It'll do.

## Day 18 - Part 1

Well, it's 2022 and I didn't get all the challenges done by end-of-year. That's life. I love Advent of Code, but late December is the worst time of year to take on extra circular activities.

But I'd still like to take a stab at completing the year. So on to day 18!

So I decided to take my input and build a tree structure to represent the data, though when I got into the weeds on this, I doubted my decision. The hardest part for me was finding the node immediately to the "left" or "right" of any given node.

Let's take a look at an example:

`[[7,[5,6]],9]`

As tree, it looks something like this:

```
           A
          / \
         B   9
        / \
       7   C
          / \
         5   6
```

Pairs are represented as letters here here. `A` is the "root" pair.

Let's say we were going to explode the `C` pair. Step one is to find the path to that pair. I do this recursively, left/depth first. Start at the root node, then follow this pattern:

1. Is this the node I'm looking for? If so, return it as a list of one item (other nodes will add themselves to this list as the stack unwinds).
2. Is this node a pair? If it's not, then it's not the node we want, return null.
3. So this is a pair. Recurse on the left branch first. Did that return something other than null? If so, add myself to the list and return that.
4. If the left branch didn't find it, recurse on the right. If that's non-null, add myself to the list and return that.
5. Otherwise, return null.

Once you've followed the process, you should get a list of nodes that represent the path, something like: `C, B, A`. That's helpful, but we'll also need to track which direction the path traversed. So tweak our algorithm to also track if it was a left or right branch, it becomes: `(C | None), (B | Right), (A | Left)`. Reading that backwards, we know we branched left at `A`, right at `B`, and `C` was our destination (no branching).

We have the node and we have the path to the node (including which side of each branch). How do we figure out the next item on the "left" or "right" of any particular node?

To figure out what's left of `C`, we need to know it's parent node, and which side `C` was on. We know the parent node is `B`, and `C` is on the Right side of `B`. So we can start our search on the *left* side of `B`, which is a value node of 7. So 7 is to the left of `C` (and `5` for that matter).

The right of `C` is a little more complicated. We need to traverse back up the path until we find a node where we traversed *left*. That node would be `A`. Now we can start looking on the *right* side of `A`. In this simple example, it's a value node, `9`.

Let's make the same example slightly more complicated:

```
             A
          /     \
         B       D
        / \     / \
       7   C   1   2
          / \
         5   6
```

All I've done is change `9` into `D` with two value nodes.

So, to find what's right of `C`, we traverse back to `A` (the first node in our history that traversed left). We look at the right branch of `A` and it's a pair, so we have to keep looking. We then recursively traverse all the left branches until we find a value, in this case, `1`.

So in general, to find something to the *right* of a node:

1. Traverse back *down* the path to the selected node until you find a node where your branched *left*.
2. From that node, start on the *right* branch. If it's a value, you're done. Otherwise, traverse all the *left* nodes until you've found a value.

The same is true of the left side of a node, but with the directions reversed.

Now we can find nodes that are "left of" or "right of" a given node. For exploding, we just add the values to those nodes (if any node is found, of course), and then convert the node in question (`C` in our example), to a value node, with a value of 0. For the more complicated example, it would end up:

```
             A
          /     \
         B       D
        / \     / \
      12   0   7   2
```

Splitting a node is much simpler, IMO. We just convert the node that's too large into a pair, and add on two appropriate value nodes. Splitting `12`, for example:

```
             A
          /     \
         B       D
        / \     / \
       E   0   7   2
      / \
     6   6
```

Finally, the explode and split methods need to return whether or not any changes were made. Then we can iterate until both methods return false.

## Day 18 - Part 2

Part two is applying the stuff you've built to the set of possible combinations. I did this with nested loops. There are a couple keys to this part:

1. As the instructions state, adding is not commutative, so A + B ??? B + A. So we have to try all permutations. I did this by not limiting my inner loop to only the items "past" the one the outer loop is referencing.
2. You can't add a number to itself. In my nested loops, I just verify that the outer and inner loop aren't pointing at the same index.
3. If you're using mutable trees and don't create clones, the reductions will effect later results. I create clone copies of each tree prior to reducing.

## Day 19 - Part 1

Oof. This one nearly broke me. But on the *fifth* attempt, I got it.

First, let's talk about strategy at a broad level, by taking a much smaller example and dropping the third dimension. Instead, let's have two scanners with three beacons each on a 2D plane. Assume the scanners overlap completely and we just need to figure out the rotation and offset. Let's call the scanners Left and Right, and start with them already rotated correctly. Left sees the numbered beacons, and Right sees the alphabetical beacons.

```
0 .......
1 .1.....
2 .2.3...
3 .......
4 ...A...
5 ...B.C.
6 .......
  0123456
```

As a human, I look at this and immediate recognize that it's the same pattern, just offset. But how do I figure that out programmatically? Think about the *delta* between the beacons in the Left scanner and the Right scanner. Take the delta between `1` and `A`: `(1,1) - (3,4)`, or `(-2,-3)`, for example.

If the pattern is the same, and they're oriented the same, then the beacons that overlap should all be the same delta from each other. That's something we can test. First we calculate the delta between all the points:

| Pts   | Coordinates   | Delta    |
| ----- | ------------- | -------- |
| 1 - A | (1,1) - (3,4) | (-2, -3) |
| 1 - B | (1,1) - (3,5) | (-2, -4) |
| 1 - C | (1,1) - (5,5) | (-4, -4) |
| 2 - A | (1,2) - (3,4) | (-2, -2) |
| 2 - B | (1,2) - (3,5) | (-2, -3) |
| 2 - C | (1,2) - (5,5) | (-4, -3) |
| 3 - A | (3,2) - (3,4) | ( 0, -2) |
| 3 - B | (3,2) - (3,5) | ( 0, -3) |
| 3 - C | (3,2) - (5,5) | (-2, -3) |

Now lets look at how frequently each delta appears:

| Delta    | Frequency |
| -------- | --------- |
| (-2, -3) | 3         |
| (-2, -4) | 1         |
| (-4, -4) | 1         |
| (-2, -2) | 1         |
| (-4, -3) | 1         |
| ( 0, -2) | 1         |
| ( 0, -3) | 1         |

So we have a delta that appears 3 times and we know we have 3 beacons that should match. It's *very* likely we found the correct offset. We can translate the Right scanner's beacons by that delta and see where we come out:

| Right Beacon | With offset       | Translated | Matches Left Beacon |
| ------------ | ----------------- | ---------- | ------------------- |
| A            | (3, 4) + (-2, -3) | (1, 1)     | Yes, 1.             |
| B            | (3, 5) + (-2, -3) | (1, 2)     | Yes, 2.             |
| C            | (5, 5) + (-2, -3) | (3, 2)     | Yes, 3.             |

So with an offset of `(-2, -3)`, all of the beacons align to another beacon. We got it. You can apply this idea to 3D space as well, just add another dimension to your calculations.

So that solves the offset problem. What about orientation? The instructions specifically mention 24 possible orientations, but I, for one, don't work with coordinates much and didn't know off the top of my head how to arrive at those orientations, or how to translate beacons to match those orientations.

So here's how I thought about it, back in 3D space: Imagine a six-sided die sitting on a table in front of you. The 1 is facing up (away from the table, looking at the sky, so to speak). In this position, depending on rotation, either 2, 3, 4, or 5 is facing you. So for the orientation of 1 facing up, there are 4 rotations. If 2 is facing up, then either 1, 3, 4, or 6 is facing you--again 4 rotations. So extrapolating, there are 6 sides that can face up, and each of those positions can be rotated 4 ways--thus 24 possible orientations.

Ok, but how do we figure out a formula for orienting points for those 24 possible orientations? Well, I did it in two steps.

First, I need to know how to translate the positions such a different side that is facing "up". Up is totally arbitrary here, but having a fixed direction in mind helps to reason about it. I did this by (very crudely) folding a piece of paper into a cube. I set the cube before me and wrote +Z on the side facing up, +X on the side to my left, and +Y on the side facing away from me (these axes were also arbitrary). Then on the opposite side for each letter, I wrote the negative of that letter. So the opposite side of +X was -X, +Y to -Y, etc. I labeled the side facing up (+Z) "0", and wrote it down. I'm using the side to my left as "X", facing away from me as "Y", and up as "Z".

`Position 0 = +X, +Y, +Z`

I then turned the cube so the +X side was facing up. I arbitrarily labeled this side "1". Now -Z was on the left side, +Y faced away, and +X was up.

`Position 1 = -Z, +Y, +X`

And then repeated for the remaining sides. These became my direction orientation step. The numbers I used as labels are not important (because I'm just trying all possible orientations), but the set of six +/- X/Y/Z values become my algorithm.

So if want to translate a point for position 1, I use it's -Z value as X, +Y as Y, and +X as Z. Something like:

`var position1Translated = new Point { X = -pt.Z, Y = pt.Y, Z = pt.X };`

Part two of this is rotation. I did basically the same thing. First, I oriented the cube so that +X was to my left, +Y faced away, +Z was up. That's rotation 0.

`Rotation 0 = +X, +Y, +Z`

Then I turned it once counter-clockwise. Now +Y was to my left, -X faced away, and +Z remained on top. (I'm rotating on the Z access, so it always stays on top)

`Rotation 1 = +Y, -X, +Z`

Repeat two more times, and that's your algorithm for rotation. For the complete orientation, I apply the direction, and then the rotation.

So finally, I combine these ideas into a single algorithm, which is (roughly speaking):

1. Take the first scanner and consider it solved. This gives me a global, solved space to match everything else against.
2. Loop through all possible orientations, and for each, loop through the unsolved scanners. Calculate the deltas of between all the beacons in the unsolved scanner and the solved global space.
3. For all the deltas that came up at least 12 times, translate all the beacons by that delta and check for matches in the global solved space. If at least 12 beacons still match, then consider that a solve.
4. Remove the solved scanner from the list of unsolved scanners. Add all of it's beacons (translated) into the solved space. Because I'm using a set, duplicates are naturally ignored.
5. Repeat until all unsolved scanners are solved.

The set of solved scanners gives me my count.

## Day 19 - Part 2

Part two required a couple small tweaks. First, we need to keep track of the solved scanners' final positions. Luckily, each scanner's position relative to the world origin (0,0,0) is just the offset we figured out for that scanner. So I keep track of each offset, which is also that scanner's position relative to the global solved space.

Then we calculate the manhattan distance between all the scanners, looking for the largest one. To get the manhattan distance, I take the delta, then add the absolute value of each coordinate of the delta together.

In retrospect, I should have named my `Beacon` type something more generic, like `Vector3` or something. It really stood out when I wrote: `knownScanners.Add(new Beacon(0, 0, 0));` -- Why am I adding a beacon to a list of scanners? I'm not. I'm adding a *position* to a list of scanner *positions*. I also use `Beacon` to represent the difference between two points. I'm surprised I don't find a way to store XML in a `Beacon` while I was at it.

Anyway, too lazy to fix.

## Day 20 - Part 1

This one threw me a wrinkle with the non-example input. I'm guessing it did for everyone. Remember that the field is infinite. So in the example input, out in the deep expanse of the infinite, there's a place that looks like this:

```
.........
..[...]..
..[...]..
..[...]..
.........
```

That 9x9 area calculates to index `0`. The example algorithm starts with : `..#.#..####`. So void spaces stay dark.

*My* algorithm started with: `#.#.....`. So that means that infinite void of darkness would all go **bright** on the first step. So then it would look like:

```
#########
##[###]##
##[###]##
##[###]##
#########
```

That calculates out to index `256`, which in my algorithm turns it all **dark**. 

In other words, the space around what you're looking at is flipping on and off for my algorithm (and likely yours too, if you're playing at home).

So the solution (for me) is admittedly lazy. I consider the bounds of my problem, and assume any point outside of my bounds is dark for odd steps (first, third), and bright on event steps (second). Further, I expand the area I'm looking at by 1 in all directions on each iteration.

## Day 20 - Part 2

My part one solution worked as-is for part two, just updated it to run 50 times.

## Day 21 - Part 1

I suspect part 2 is where the puzzle begins on this one. Part 1 is mostly a matter of following directions. I can only hope that the way I modeled part 1 is somewhat useful for whatever changes I will have to make for part 2.

## Day 21 - Part 2

Part two reminds me of [Day 6](#day-6---part-2). My modeling didn't work very well here, since I put the game logic into individual models and made everything mutable. For part two, it was easier to use immutable models, with value equality instead of reference equality, with the logic separate from the models. Further, in part 1 made the game agnostic to the number of players or the type of die, both of which turned out useless. *Sigh.* "You ain't gonna need it."

Anyway, instead of modelling individual games, I had to think of it in terms of how many universes were there for each possible game state. Similar to day 6, where we modeled the population of fish, rather than the individual fish.

Think of it this way. The initial example `(4, 0) and (8, 0)` occurs in exactly one universe. What happens when player one rolls the die? That universe diverges by all the possible outcomes of rolling a 3-sided die three times.

So what are all those possible outcomes? Well, the raw outcomes are:

| Die     | roll    | outcomes  |
| ------- | ------- | --------- |
| 1, 1, 1 | 2, 1, 1 | 3, 1, 1   |
| 1, 1, 2 | 2, 1, 2 | 3, 1, 2   |
| 1, 1, 3 | 2, 1, 3 | 3, 1, 3   |
| 1, 2, 1 | 2, 2, 1 | 3, 2, 1   |
| 1, 2, 2 | 2, 2, 2 | 3, 2, 2   |
| 1, 2, 3 | 2, 2, 3 | 3, 2, 3   |
| 1, 3, 1 | 2, 3, 1 | 3, 3, 1   |
| 1, 3, 2 | 2, 3, 2 | 3, 3, 2   |
| 1, 3, 3 | 2, 3, 3 | 3, 3, 3   |

Or 27 possibilities. But we're adding up the results of those rolls, and there will be duplicates. So rather than worrying about the individual rolls, let's look at the totals:

| Die     | roll    | totals    |
| ------- | ------- | --------- |
| 3       | 4       | 5         |
| 4       | 5       | 6         |
| 5       | 6       | 7         |
| 4       | 5       | 6         |
| 5       | 6       | 7         |
| 6       | 7       | 8         |
| 5       | 6       | 7         |
| 6       | 7       | 8         |
| 7       | 8       | 9         |

Count up the frequencies of each total and you get:

| Total | Times Appeared |
| ----- | -------------- |
| 3     | 1              |
| 4     | 3              |
| 5     | 6              |
| 6     | 7              |
| 7     | 6              |
| 8     | 3              |
| 9     | 1              |

So effectively, there are seven possible outcomes, most of which occur multiple times. The `3` outcome will occur once, the `4` outcome will occur three times, etc. Thus splintering into that number of universes.

So for player one's first roll, we splinter into seven possible outcomes. The first outcome (rolls a `3`) occurs in one universe, the second output (rolls a `4`) occurs in three universes, etc. If the player wins on that roll, then we can track that. Otherwise, we move on to player two, and go through all seven possible outcomes for player two. If any of those win, we can track that. Otherwise, we end up with new game states and universe counts for those game states. We track those and iterate on the list until all possible game states have ended.

That's the broad idea anyway. For a more detailed explanation, it's probably better to read the code.

## Day 22 - Part 1

It may be possible to solve part one by tracking each individual point in the cuboid space, especially considering the -50 to +50 constraint on all axes, but this late in the challenge, one can generally assume that naive solutions will only work for part one.

Rather, I ended up thinking about it as partitioning space, where swathes of space are either turned on, or off. Well, because off is the default state of everything, I really only need to care about spaces that are on. So the the question becomes what to do when two on states overlap. For example, in 2D space:

```
+---------+
|         |
|         |
|     +---|----+
|     |   |    |
+---------+    |
      |        |
      +--------+
```

These commands overlap and create: 

```
+---------+
|         |
|         |
|         +----+
|              |
+-----+        |
      |        |
      +--------+
```

But that irregular shape is hard to reason about, hard to calculate the area of, and hard to do boolean operations on. I'd rather keep everything in as rectangles. Well, this shape can be made of rectangles:


```
+-----+---+
|     |   |
|     |   |
|     +---+----+
|     |        |
+-----+        |
      |        |
      +--------+
```

Now instead of tracking shapes, we're tracking rectangles. My set of rectangles can represent all the points that are currently turned on.

But how do we arrive at this shape? How do we decide how to do the splits. Let's take a worst-case scenario where we need to punch a whole in a rectangle, again in 2D space:

```
+----------------+
|                |
|                |
|    +----+      |
|    |    |      |
|    |    |      |
|    +----+      |
|                |
|                |
+----------------+
```

The inner rectangle is being subtracted from the outer. How do we split that up? First we remove the outer rectangle, then rebuild the remaining parts, one side at a time. Start with the left side:

```
+---+
|   |
|   |
|   |+----+
|   ||    |
|   ||    |
|   |+----+
|   |
|   |
+---+
```

This new box is the original outer box's full height, and the width is the outer box's min X to the inner box's min y - 1. We can do the same for the right side:

```
+---+      +-----+
|   |      |     |
|   |      |     |
|   |+----+|     |
|   ||    ||     |
|   ||    ||     |
|   |+----+|     |
|   |      |     |
|   |      |     |
+---+      +-----+
```

This new side is the full height again, and the width is the inner box's max X + 1, to the outer box's max x. Fill in the top next:

```
+---++----++-----+
|   ||    ||     |
|   |+----+|     |
|   |+----+|     |
|   ||    ||     |
|   ||    ||     |
|   |+----+|     |
|   |      |     |
|   |      |     |
+---+      +-----+
```

This time the width is the inner box's width (actually, it's the width of the outer box constrained by the inner box, which in this scenario happens to end up being the inner box's width). The height is the outer box's min Y to the inner box's min Y + 1. We do the last side:

```
+---++----++-----+
|   ||    ||     |
|   |+----+|     |
|   |+----+|     |
|   ||    ||     |
|   ||    ||     |
|   |+----+|     |
|   |+----+|     |
|   ||    ||     |
+---++----++-----+
```

Similarly, it's the inner box's width, and the height is the inner box's max Y + 1 to the outer box's max Y. Because this is subtraction, the final result becomes:

```
+---++----++-----+
|   ||    ||     |
|   |+----+|     |
|   |      |     |
|   |      |     |
|   |      |     |
|   |      |     |
|   |+----+|     |
|   ||    ||     |
+---++----++-----+
```

Four final boxes. You can play around with other scenarios to find edge cases. Ultimately, I generalized the idea to calculate the potential box for each side, and then remove it if the resulting box took up zero or negative space (which happens when the box being subtracted isn't fully contained by the outer box). Then extend this to another dimension for the cuboids.

Once you have subtraction figured out, each step becomes either:

1. On: Subtract the space where the new cuboids would be, and then add an "on" region in the resulting space.
2. Off: Subtract the space where the new cuboids would be.

Once that's done, calculating the total number of cuboids is a matter of summing up the total area of all the cubes of space.

## Day 22 - Part 2

A 32-bit integer should be enough for anybody! But in this case, a 64-bit integer was required. Beyond removing the constraint and making my area calculation use a larger numeric size, my solution worked as-is.

## Day 23 - Part 1

There was quite a bit to this one. Here are my key takeaways:

1. Simplify the problem. I tried a few ways to represent the "state" of the world. I had a binary map, I had nodes in a connected graph, all kinds of terrible ideas. I settled on just keeping track of each Amphipod (which I took to calling pods because I kept mistyping amphipod), and representing them as a "location" (basically the X axis), and a "depth" (basically the Y axis). If depth was zero, the pod was in the hallway, otherwise it was in a room. Rooms were at locations 2, 4, 6, and 8, and had possible depth of two. The rules allowed for a pod to leave a room and enter a room in the same "turn", but it didn't change the cost, so I assumed that any movement was either from a room to the hallway, or from the hallway to a room.
2. To find a solve, I basically shoe-horned Dijkstra's algorithm, and probably not in the most efficient way. I treated each possible state as a "node" in a graph, then keep looking at the shortest paths until I found a state in which the game is won. It probably would have been smarter to apply some heuristics to more intelligently chose the "short" paths (in other words, use A*), but it ran in a reasonable amount of time. I calculate the possible states and costs of those states based on the current state of the node I'm "visiting" in the algorithm.
3. Calculating each possible state is largely an exercise in paying very close attention to the rules.
4. Because I'm representing state as an array of Pods, and I wanted to use a dictionary to lookup if a given state had been found before (and at a lower cost), and because arrays in C# use reference identity, I had to create an `IEqualityComparer<>` implementation for my state structure, otherwise no two states (even if their values were identical) would ever match as a key. 

## Day 23 - Part 2

Luckily, most of my algorithm wasn't too hard to adapt to additional room depth. Just changing a few instances of `2` to `MaxRoomDepth` and another couple minor adjustments, and the algorithm worked.

I admit, though, that I just updated my input rather than inject two additional lines at runtime like I'm supposed'ta.

## Day 24 - Part 1

This is one of those I don't love. For me, it was all about analyzing the input, understanding what the input is doing, and then writing code that mostly ignores the input calculates results in its own way. It just feels like a hack, I guess? 

By the way, I'm not too proud to admit that I had to read discussions around this one to finally see it.

Anyway, if you carefully examine the input, you'll find it's a pattern that repeats 14 times, once for every digit. Each repeated pattern (which I've taken to calling a "processor") reads in the input and does mostly similar calculations on it. You may also notice that `w` is always used for storing the digit that's being processed (the input), `x` and `y` get reset for each processor, and `z` is the only value that carries over from one processor to the next.

Next you'd want to examine one of the processors closely. Here's the first one from my input:

```
inp w
mul x 0
add x z
mod x 26
div z 1
add x 14
eql x w
eql x 0
mul y 0
add y 25
mul y x
add y 1
mul z y
mul y 0
add y w
add y 1
mul y x
add z y
```

You can kind of step through the process and keep track of roughly what's happening. For example, the first few instructions:

```
[  0] inp w      | w = input
[  1] mul x 0    | x = 0
[  2] add x z    | x = z
[  3] mod x 26   | x = z % 26
[  4] div z 1    | z = z (effectively a no-op)
[  5] add x 14   | x = (z % 26) + 14
[  6] eql x w    | (z % 26) + 14 == input ? 1 : 0
```

At this point `(z % 26) + 14 == input ? 1 : 0`, it's important to remember that `input` is a number between 1 and 9 inclusive. `(z % 26)` is going to produce a number between 0 and 25, which is added to 14. There is no value for z which will result in that side of the equation producing a number less than 14. So it will never be true. So we can ignore all that math, and just write 0 there, and continue.

```
[  6] eql x w    | x = 0
[  7] eql x 0    | x = 1
[  8] mul y 0    | y = 0
[  9] add y 25   | y = 25
[ 10] mul y x    | y = 25 (x is 1, so this is a no-op)
[ 11] add y 1    | y = 26 (another no-op)
[ 12] mul z y    | z = 26 * z
[ 13] mul y 0    | y = 0
[ 14] add y w    | y = input
[ 15] add y 1    | y = input + 1
[ 16] mul y x    | y = input + 1 (x is 1, so this is a no-op)
[ 17] add z y    | z = (26 * z) + (input + 1)
```

Since `z` is the only value that carries over, the formula for this unit is `z = (26 * z) + (input + 1)`. The second processor in my input is similar, except it works out to `z = (26 * z) + (input + 7)`. In total, there are 7 processors that follow this flow, which the value being added to `input` varying.

There is another variation on this pattern that is a bit more complex. Here's an example from my input:

```
[  0] inp w      | w = input
[  1] mul x 0    | x = 0
[  2] add x z    | x = z
[  3] mod x 26   | x = z % 26
[  4] div z 26   | z = z / 26
[  5] add x -6   | x = (z % 26) + -6
[  6] eql x w    | x = ((z % 26) + -6) == input ? 1 : 0
```

Unlike in the previous processor, this conditional (`((z % 26) + -6) == input`) could potentially match something. In fact, we can calculate what input value could make this statement true for a given z value. But from here, we must branch into two possible values for x, shown separated by a colon below.

```
[  6] eql x w    | x = 1 : 0
[  7] eql x 0    | x = 0 : 1
[  8] mul y 0    | y = 0
[  9] add y 25   | y = 25
[ 10] mul y x    | y = 0 : 25
[ 11] add y 1    | y = 1 : 26
[ 12] mul z y    | z = (z / 26) : (z / 26) * 26
[ 13] mul y 0    | y = 0
[ 14] add y w    | y = input
[ 15] add y 10   | y = input + 10
[ 16] mul y x    | y = 0 : input + 10
[ 17] add z y    | z = (z / 26) : ((z / 26) * 26) + (input + 10)
```

So depending on the state of z and digit fed to the processor, we either get `z / 26`, or `((z / 26) * 26) + (input + 10)`. Other processors in the input come a similar result:  `z / 26` or `((z / 26) * 26) + (input + 13)`, for example.

But how does that get us closer to finding model numbers that work? Well, we know Z must reach zero by the end of the processing. We know that our first type of processors follow this equation: `z = (26 * z) + (input + A)` where `A` appears to always be a positive number. This will only ever increase Z by a factor of roughly 26, and will happen seven times.

We need Z to be zero, so we must also decrease Z by a factor of roughly 26. `z / 26` accomplishes this, but `((z / 26) * 26) + (input + B)`, where `B` always appears to be a positive number, does not. So we need the second processor to always chose the `z / 26` path, and because we know the conditions to chose that path (`((z % 26) + C) == input`, where `C` appears to always be negative), we calculate the input digit to cause that to happen.

So, what I do is parse the input, detect each type of processor, and build a model around that. Then I iterate through all the processors. For the "simple" processors, I just brute-force the input digits. For the "complex" processors, I calculate which (if any) input digit would produce the valid path. This reduces my problem space from 9^14, to 9^7, making it solvable in a reasonable amount of time.

## Day 24 - Part 2

I just reversed the order of how I calculate the brute-force digits. It takes a little longer than part 1, but still fast enough to be acceptable.

## Day 25 - Part 1

I always wondered if day 25 would be the hardest, or if the AoC gods would have mercy on Christmas Eve. I feel like they had mercy. So for this one, I once again just stuffed all the data into a 1 dimensional array and wrote some helpers around accessing the data. For each step, I make an empty map, copy over the group that is stationary, then move (or not) the group that is moving. I made a few small tweaks to improve performance:

1. I use a poor man's array pool. Rather than creating new arrays and then immediately throwing them away, I toss them into a stack when I'm done with them, and pop them off when I need a new one... In reality, the stack only ever has one item in it so I could have just swapped between a pair of arrays, but that was just more logic.
2. Since I'm using a pool, I need to initialize the array. I could have to just looped through all the elements and set them to '.' manually, but instead I create an "empty" array, initialized to all '.' elements, and then copy that array into the array I want to zero out.

Did these optimizations make a significant difference? No idea. I broke the golden rule of "performancing" and added tweaks without profiling. In any case, it arrives at the answer in a reasonable amount of time.

And... I won't spoil part 2, except to say if you solved Part 1, I'm quite confident you can solve part 2.
