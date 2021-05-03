# blocks
Sample of multithreaded file processing(potentially very big one which doesn't fit into RAM), using the following approach
1. Input file is splitted onto parts
2. those parts are distributed accross of working threads
3. next they accumulate in order

There is also control involved in the process, so data from input file would be retrieved to fill max 2x of worker threads and accumulator queue increased no more to 2x of worker threads.


## Project schema
### Common types
Is the project defining main idea of blocks approach, so consumer projects need only supply execution blocks to it without need to deal with all multithreading work.

### File signature
It using `Common types` project to generate file signature by computing SHA256 hashes of blocks of required size along with their numbers

### Archiver
Parallel GZip archiver which is able to compress and decompress the files by splitting them to blocks of size of 1MiB.
