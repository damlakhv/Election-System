# Features

- **Voting**: Even if people want, they cannot vote on someone else's behalf due to timestamp, ID verification, and other features.
- **Candidate Info**: Voters can see information about candidates and their teams (e.g., deputy mayor).
- **Result**: A result screen is displayed, allowing users to easily see election outcomes across cities and regions.
- **Graphs**: Election results are visualized with graphs, showing changes from past elections to today.
- **Admin Page**: In addition to the voter page, the UI includes an Admin Page, enabling actions like monitoring voter activity and more.

### Additional Features

- [Design and Structure](https://drive.google.com/file/d/1EHyqmECYvRcBHAIpEP9eZkzTthbCX0Bs/view?usp=sharing)
- [EER Model](https://drive.google.com/file/d/1UjPAjPc4bHRRbZDrr0SW4F-T9SFqRrcD/view?usp=sharing)
- [Project Proposal](https://drive.google.com/file/d/1GMw8k160h1TrwF1qwA4WmWbfM8pCgdF_/view?usp=sharing)

### Updated EER and Schema
-[Final EER and Schema](https://drive.google.com/file/d/1Obvlm7CrFCieCiXfS1v1WJRVhM6rs9aU/view?usp=drive_link)


## Installation

Refer to the [Installation Guide](https://youtu.be/kKiwN4ueqQM).

## Requirements

- **Database**: Microsoft SQL Server
- **Programming Language**: C#
- **Development Environment**: Visual Studio 2022

### Frameworks

- Entity Framework (EF), LINQ

## Discussion

The project was developed by a group of four students. The idea for the project, "Election System," was collectively decided upon. Here are the key steps of the development process:

1. **Proposal and Feature Listing**: Created a project proposal and listed numerous features, many of which were successfully implemented, some of them were changed or removed.
2. **EER Model Design**: Initially designed an ER model, which was later extended to an Enhanced ER (EER) model to include inherited entities. Most attributes and entities were preserved except one as stated above.
3. **Database Design**: Completed the Modeling and Structure phase, normalized tables to 3NF, and created tables, triggers, indexes, and constraints in Microsoft SQL Server.
4. **Data Population**: Wrote a Java program to generate insert statements, which were then applied in SQL Server.
5. **Backend Development**: Connected the database using Entity Framework and utilized LINQ for data handling.
6. **UI Development**: Initially designed the UI using Blazor but switched to C# Windows Forms due to challenges. Successfully connected EF and LINQ with the UI.

### Outcome

The Election System Project was completed successfully with all models and designs implemented effectively.



