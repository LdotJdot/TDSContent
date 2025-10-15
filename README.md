# TDSContent: Ultra-Fast File Full-Text Search
  
## Introduction
`TDSContent` (version c1.0.5) is a powerful, open-source file content search tool designed to deliver lightning-fast search results in milliseconds. Built with `C#` based on `Lucene` engine and featuring a sleek interface crafted using `Avalonia`, TDSContent stands out from traditional full-disk indexing tools by offering targeted and efficient searching capabilities. It is the sister project of the `TDS` file name search tool (https://github.com/LdotJdot/TDS), complementing its functionality by addressing the need for file content search.

Unlike traditional tools that index entire disks, TDSContent allows users to specify particular folder directories and select specific file extensions. It then automatically creates indexes in the background, enabling users to quickly locate and match text within files. This targeted approach ensures that searches are both quick and accurate, making it easy to find the exact content you need.

## 💥 Key Advantages and Features
* **Lightning** - Fast Search Speeds: Experience millisecond - level search speeds that leave conventional tools in the dust. TDSContent delivers instant results, so you can find the exact information you need in a blink.
* **Precision** Targeting: Unlike other tools that indiscriminately index your entire drive, TDSContent empowers you to take control. Specify the particular folder directories and select the file extensions you care about most. This focused approach ensures that your searches are hyper - efficient and tailored to your unique needs.
* **Automatic Indexing**: Set it and forget it! TDSContent works seamlessly in the background to automatically create indexes. Once you’ve designated your target folders and extensions, the tool takes over, ensuring that your files are always ready for lightning - fast retrieval.
* **Extensive File Format Support**: TDSContent is designed to handle a wide array of file formats, including docx, pptx, dwg, dxf, pdf, txt, md, and all pain type file e.g. json, md, ini and more. No matter what kind of documents you deal with daily, TDSContent has got your back.

✨ Whether you’re a developer, a content creator, or a power user striving for peak efficiency, TDSContent is your ultimate ally in the quest for instant, precision file content search. Dive in and see for yourself how TDSContent transforms the way you work and unleashes your true potential.

## How to do?
 Right click and add the folder to index. Browse the path and choose the file extensions. Wait for the index finished then type the keywords.
 
 * If the file name and icon are incorrect, you can click "Refresh USN".
 * If the file content not updated, you can click  "Reindex the selected files".
 * If the results not updated, you can click "Reindex the selected folders", then program will rescan the content in the folder as new.

<img width="602" height="429" alt="image" src="https://github.com/user-attachments/assets/a1b04530-c6ab-4aed-b1b4-1f6468671344" />

Fast content search within files in select folder (sub-folders).

<img width="422" height="492" alt="image" src="https://github.com/user-attachments/assets/3602b0fa-0bd5-423e-b559-3945f8190621" />

Developers can also add their own file to string Converter to support custom indexing for additional formats.

## License
TDSContent is licensed under the MIT License. However, it includes components that are licensed under the Apache License, Version 2.0. Please review the licenses of all components used in the project to ensure compliance.

This project includes components licensed under the Apache License, Version 2.0.
- Lucene.Net.Analysis.SmartCn: https://lucenenet.apache.org
- PDFiumCore: https://github.com/Dtronix/PDFiumCore

## Contribution Guide
We welcome any developers to participate in the development and improvement of TDSContent. You can contribute to the project by submitting Issues, Pull Requests, or joining discussions.

## Contact
For any questions, suggestions, or feedback, please feel free to open an issue on this repository or contact us directly.
We hope TDSContent can help you manage and search file contents more efficiently and enjoyable!
