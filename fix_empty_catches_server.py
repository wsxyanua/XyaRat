#!/usr/bin/env python3
"""
Automated script to fix empty catch blocks in Server C# files
Replaces 'catch { }' with proper ErrorHandler calls
"""

import os
import re
import sys

def fix_empty_catches(file_path):
    """Fix all empty catch blocks in a single file"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        fixes_made = 0
        
        # Pattern 1: catch { } on same line
        pattern1 = r'catch\s*\{\s*\}'
        
        # Pattern 2: catch on one line, { } on next lines (with optional indentation)
        pattern2 = r'catch\s*\n(\s*)\{\s*\n\s*\}'
        
        # Extract context to determine if it's UI operation (Forms) or other
        is_form_file = 'Forms/' in file_path or 'Form' in os.path.basename(file_path)
        
        # Replace pattern 1 (single line)
        matches = list(re.finditer(pattern1, content))
        for match in reversed(matches):  # Reverse to maintain positions
            # Get surrounding context
            start = max(0, match.start() - 200)
            end = min(len(content), match.end() + 100)
            context = content[start:end]
            
            # Determine method name from context
            method_match = re.search(r'(\w+)\s*\([^)]*\)\s*\{[^{]*' + re.escape(content[match.start():match.end()]), 
                                    content[:match.end()])
            
            method_name = "unknown_method"
            if method_match:
                # Extract method name
                prev_text = content[max(0, match.start()-300):match.start()]
                method_search = re.findall(r'(?:public|private|protected|internal)?\s*(?:static)?\s*(?:void|bool|string|int|\w+)\s+(\w+)\s*\(', prev_text)
                if method_search:
                    method_name = method_search[-1]
            
            # Build replacement
            replacement = f"""catch (Exception ex)
            {{
                ErrorHandler.HandleNonCritical(() => {{ }}, ex, "{method_name} failed");
            }}"""
            
            content = content[:match.start()] + replacement + content[match.end():]
            fixes_made += 1
        
        # Replace pattern 2 (multiline)
        matches = list(re.finditer(pattern2, content))
        for match in reversed(matches):
            indent = match.group(1)
            
            # Determine method name
            prev_text = content[max(0, match.start()-300):match.start()]
            method_search = re.findall(r'(?:public|private|protected|internal)?\s*(?:static)?\s*(?:void|bool|string|int|\w+)\s+(\w+)\s*\(', prev_text)
            method_name = method_search[-1] if method_search else "unknown_method"
            
            replacement = f"""catch (Exception ex)
{indent}{{
{indent}    ErrorHandler.HandleNonCritical(() => {{ }}, ex, "{method_name} failed");
{indent}}}"""
            
            content = content[:match.start()] + replacement + content[match.end():]
            fixes_made += 1
        
        # Only write if changes were made
        if fixes_made > 0 and content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"✓ Fixed {fixes_made} empty catch blocks in {os.path.basename(file_path)}")
            return fixes_made
        else:
            return 0
            
    except Exception as e:
        print(f"✗ Error processing {file_path}: {e}")
        return 0

def main():
    """Main execution"""
    server_dir = "/home/logntsu/Downloads/XyaRat/Server"
    
    if not os.path.exists(server_dir):
        print(f"Error: Server directory not found: {server_dir}")
        return
    
    # Target directories
    target_dirs = [
        os.path.join(server_dir, "Forms"),
        os.path.join(server_dir, "Handle Packet"),
        os.path.join(server_dir, "Helper"),
        os.path.join(server_dir, "Connection"),
    ]
    
    total_files = 0
    total_fixes = 0
    
    for target_dir in target_dirs:
        if not os.path.exists(target_dir):
            continue
            
        for root, dirs, files in os.walk(target_dir):
            for file in files:
                if file.endswith('.cs'):
                    file_path = os.path.join(root, file)
                    fixes = fix_empty_catches(file_path)
                    if fixes > 0:
                        total_files += 1
                        total_fixes += fixes
    
    print(f"\n{'='*60}")
    print(f"Summary: Fixed {total_fixes} empty catch blocks across {total_files} files")
    print(f"{'='*60}")

if __name__ == "__main__":
    main()
