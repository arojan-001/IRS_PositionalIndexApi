# IRS_Positionalindex
Positional index is a data structure that stores the positions of each term in a document collection, enabling fast and accurate retrieval of phrase queries.
A positional index is an extension of an inverted index that also stores the positions of each term in each document, usually as a list of integers. 
For example, the term "hello" might have the following posting list: [doc3: 120, 125, 278; doc5: 28; doc10: 132, 182 etc.]. 
This means that the term "hello" appears in doc3 at positions 120, 125, and 278, in doc5 at position 28, and in doc10 at positions 132 and 182 etc.
A positional index can answer phrase queries by intersecting the posting lists of the terms in the phrase and checking if their positions are consecutive or within a certain distance.
![postings_pos](https://github.com/arojan-001/IRS_Positionalindex/assets/61918182/4d3e9e3f-8d47-445a-bfc2-c7997ff08e77)
