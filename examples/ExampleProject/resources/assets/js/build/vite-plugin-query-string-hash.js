import fs from 'fs';

function addQueryStringHash(fileName) {
    if (fileName.includes('[hash]')) {
        return fileName;
    }
    return fileName + '?v=[hash]';
}

export default function queryStringHash() {

    const fileNames = {};

    return {
        name: 'query-string-hash',
        outputOptions(outputOptions) {
            outputOptions.entryFileNames = addQueryStringHash(outputOptions.entryFileNames);
            outputOptions.chunkFileNames = addQueryStringHash(outputOptions.chunkFileNames);
            outputOptions.assetFileNames = addQueryStringHash(outputOptions.assetFileNames);
            
            return outputOptions
        },
        generateBundle: {
            order: 'post',
            handler: function (options, bundle, isWrite) {
                for (let file in bundle) {
                    let queryStringPos = bundle[file].fileName.indexOf('?');
                    if (queryStringPos > -1) {
                        fileNames[file] = bundle[file].fileName;
                        bundle[file].fileName = bundle[file].fileName.substring(0, queryStringPos);
                    }
                }
                //console.log('BUNDLE: ', JSON.stringify(bundle, null, 2));
            }
        },
        writeBundle: {
            order: 'pre',
            sequential: true,
            handler: function (options, bundle) {
                for (let file in bundle) {
                    if (fileNames[file]) {
                        bundle[file].fileName = fileNames[file];
                    }
                }
            }
        }
    }
}
