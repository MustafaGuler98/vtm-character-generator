const PDF_SERVICE_URL = 'https://vtm-scribe-service.vercel.app/generate-pdf';

export class RateLimitError extends Error {
    constructor(retryAfterSeconds) {
        super(`Too many characters were generated at once. Please wait ${retryAfterSeconds} seconds and try again.`);
        this.name = 'RateLimitError';
    }
}

export async function fetchCharacter(url, options = {}) {
    const response = await fetch(url, options);

    if (response.status === 429) {
        const retryAfterHeader = Number.parseInt(response.headers.get('Retry-After') ?? '', 10);
        const retryAfterSeconds = Number.isFinite(retryAfterHeader) ? retryAfterHeader : 60;
        throw new RateLimitError(retryAfterSeconds);
    }

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
}

export async function fetchCharacterOptions() {
    const response = await fetch('/api/character/options');

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
}

export async function generatePdf(characterData, onProgress) {
    const response = await fetch(PDF_SERVICE_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(characterData)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Server Error (${response.status}): ${errorText}`);
    }

    if (!response.body) {
        throw new Error('The PDF response did not contain a readable body.');
    }

    const reader = response.body.getReader();
    const contentLengthHeader = response.headers.get('Content-Length');
    const contentLength = contentLengthHeader
        ? Number.parseInt(contentLengthHeader, 10)
        : null;
    const chunks = [];
    let receivedLength = 0;

    while (true) {
        const { done, value } = await reader.read();
        if (done) {
            break;
        }

        chunks.push(value);
        receivedLength += value.length;

        if (contentLength) {
            onProgress({
                percent: Math.round((receivedLength / contentLength) * 100),
                receivedKilobytes: null
            });
        } else {
            onProgress({
                percent: null,
                receivedKilobytes: Math.round(receivedLength / 1024)
            });
        }
    }

    return new Blob(chunks, { type: 'application/pdf' });
}
