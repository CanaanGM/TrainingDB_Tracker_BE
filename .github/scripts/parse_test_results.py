import xml.etree.ElementTree as ET
import json
import os

def parse_trx(file_path):
    tree = ET.parse(file_path)
    root = tree.getroot()

    namespace = {
        '': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'
    }

    summary = {
        'passed': 0,
        'failed': 0,
        'skipped': 0
    }

    for unit_test_result in root.findall('.//Result', namespace):
        outcome = unit_test_result.get('outcome')
        if outcome == 'Passed':
            summary['passed'] += 1
        elif outcome == 'Failed':
            summary['failed'] += 1
        elif outcome == 'NotExecuted':
            summary['skipped'] += 1

    with open('test_summary.json', 'w') as json_file:
        json.dump(summary, json_file, indent=2)

    return summary

def update_readme(summary):
    badge_url = (
        "https://img.shields.io/badge/tests-{status}-brightgreen"
        if summary['failed'] == 0
        else "https://img.shields.io/badge/tests-{status}-red"
    ).format(status="passing" if summary['failed'] == 0 else "failing")

    with open("README.md", "r") as readme_file:
        readme_content = readme_file.readlines()

    for i, line in enumerate(readme_content):
        if line.startswith("![Test Status]"):
            readme_content[i] = f"![Test Status]({badge_url})\n"
            break
    else:
        readme_content.insert(0, f"![Test Status]({badge_url})\n")

    with open("README.md", "w") as readme_file:
        readme_file.writelines(readme_content)

if __name__ == '__main__':
    summary = parse_trx('test_results.trx')
    update_readme(summary)
