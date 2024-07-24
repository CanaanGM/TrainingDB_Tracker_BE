# .github/scripts/parse_test_results.py

import xml.etree.ElementTree as ET
import json

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

if __name__ == '__main__':
    parse_trx('test_results.trx')
