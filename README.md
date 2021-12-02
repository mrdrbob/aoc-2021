# Bob's Advent of Code 2021

So here's the deal. I'm doing Advent of Code 2021. I'm also still doing Advent of Code 2020 (got 4.5 challenges remaining). I was hoping to wrap up 2020 before 2021 started, but here we are. Typically, I've done these in Rust because it's fun and this is a low-pressure opportunity to play around with a language I don't normally get to use. But for a number of reasons, I'm just short on time this year and will just be doing C#. Deal with it.

## Day 1 - Part 1

Day one is usually kind of a warmup exercise. Like, hey, remember code?

This could be written as a one-liner using tuples, but I created a small `Accumulator` record class just to make things clearer. Applied this to an aggregator and BOOM.

`Aggregate` is one of those LINQ features too few people know about (and I was one of those people for quite a long time). Want to write your own sum for "sum" reason? `list.Aggregate(0, (acc, val) => acc + val)`. Want to multiply instead of add? `list.Aggregate(1, (acc, val) => acc * val)`. Lots of stuff you can do in not a lot of code.

## Day 1 - Part 2

This was largely a matter of applying a rolling queue to the values and yielding the sum of the queue when it was full. I wrapped this logic up into an extension method that I could then just drop in the LINQ chain.
