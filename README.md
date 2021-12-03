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
