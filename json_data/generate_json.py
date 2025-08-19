#!/usr/bin/env python3
"""
Script to generate sample JSON files of specified size in megabytes.
Usage: python generate_json.py <size_in_mb> [output_filename]
"""

import json
import sys
import os
import argparse
from typing import List, Dict, Any


def generate_test_object(id_value: int) -> Dict[str, Any]:
    """Generate a single test object similar to TestDto structure."""
    return {
        "Id": id_value,
        "Name": f"Test {id_value}",
        "Value": id_value
    }


def estimate_object_size() -> int:
    """Estimate the size of a single test object when serialized to JSON."""
    sample_obj = generate_test_object(999999)  # Use a large ID to get max size estimate
    sample_json = json.dumps(sample_obj, separators=(',', ':'))
    return len(sample_json.encode('utf-8'))


def generate_json_file(target_size_mb: float, output_filename: str) -> None:
    """
    Generate a JSON file of approximately the specified size in megabytes.
    
    Args:
        target_size_mb: Target size in megabytes
        output_filename: Output file name
    """
    target_size_bytes = int(target_size_mb * 1024 * 1024)
    
    # Estimate how many objects we need
    estimated_obj_size = estimate_object_size()
    estimated_count = max(1, target_size_bytes // estimated_obj_size)
    
    print(f"Target size: {target_size_mb} MB ({target_size_bytes:,} bytes)")
    print(f"Estimated object size: {estimated_obj_size} bytes")
    print(f"Estimated object count: {estimated_count:,}")
    
    # Generate objects and write to file
    print("Generating JSON data...")
    
    with open(output_filename, 'w', encoding='utf-8') as f:
        f.write('[')
        
        current_size = 1  # Start with opening bracket
        obj_count = 0
        
        while current_size < target_size_bytes:
            obj = generate_test_object(obj_count)
            obj_json = json.dumps(obj, separators=(',', ':'))
            
            # Add comma if not the first object
            if obj_count > 0:
                f.write(',')
                current_size += 1
            
            f.write(obj_json)
            current_size += len(obj_json.encode('utf-8'))
            obj_count += 1
            
            # Progress indicator for large files
            if obj_count % 10000 == 0:
                current_mb = current_size / (1024 * 1024)
                print(f"Progress: {current_mb:.2f} MB / {target_size_mb} MB ({obj_count:,} objects)")
        
        f.write(']')
        current_size += 1
    
    # Get actual file size
    actual_size = os.path.getsize(output_filename)
    actual_size_mb = actual_size / (1024 * 1024)
    
    print(f"\nFile generated successfully!")
    print(f"Output file: {output_filename}")
    print(f"Objects created: {obj_count:,}")
    print(f"Target size: {target_size_mb} MB")
    print(f"Actual size: {actual_size_mb:.2f} MB ({actual_size:,} bytes)")
    print(f"Size difference: {abs(actual_size_mb - target_size_mb):.2f} MB")


def main():
    parser = argparse.ArgumentParser(
        description="Generate sample JSON files of specified size in megabytes",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python generate_json.py 10                    # Generate 10MB file as 'large_json.json'
  python generate_json.py 5.5 my_data.json     # Generate 5.5MB file as 'my_data.json'
  python generate_json.py 100 test_100mb.json  # Generate 100MB file
        """
    )
    
    parser.add_argument(
        'size_mb',
        type=float,
        help='Target file size in megabytes (e.g., 10, 5.5, 100)'
    )
    
    parser.add_argument(
        'output_filename',
        nargs='?',
        default='large_json.json',
        help='Output filename (default: large_json.json)'
    )
    
    parser.add_argument(
        '--pretty',
        action='store_true',
        help='Generate pretty-printed JSON (will be larger than target size)'
    )
    
    args = parser.parse_args()
    
    if args.size_mb <= 0:
        print("Error: Size must be greater than 0 MB")
        sys.exit(1)
    
    try:
        if args.pretty:
            generate_pretty_json_file(args.size_mb, args.output_filename)
        else:
            generate_json_file(args.size_mb, args.output_filename)
    except KeyboardInterrupt:
        print("\nOperation cancelled by user")
        sys.exit(1)
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)


def generate_pretty_json_file(target_size_mb: float, output_filename: str) -> None:
    """Generate a pretty-printed JSON file (larger but more readable)."""
    target_size_bytes = int(target_size_mb * 1024 * 1024)
    
    print(f"Generating pretty-printed JSON file...")
    print(f"Target size: {target_size_mb} MB (approximate, will be larger due to formatting)")
    
    # For pretty printing, we need fewer objects as formatting adds size
    estimated_obj_size = estimate_object_size() * 3  # Rough estimate for pretty printing
    estimated_count = max(1, target_size_bytes // estimated_obj_size)
    
    objects = []
    for i in range(estimated_count):
        objects.append(generate_test_object(i))
    
    with open(output_filename, 'w', encoding='utf-8') as f:
        json.dump(objects, f, indent=2, separators=(',', ': '))
    
    actual_size = os.path.getsize(output_filename)
    actual_size_mb = actual_size / (1024 * 1024)
    
    print(f"\nPretty JSON file generated!")
    print(f"Output file: {output_filename}")
    print(f"Objects created: {len(objects):,}")
    print(f"Actual size: {actual_size_mb:.2f} MB ({actual_size:,} bytes)")


if __name__ == "__main__":
    main() 