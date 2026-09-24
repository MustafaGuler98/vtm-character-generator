import {
    fetchCharacter,
    fetchCharacterOptions,
    generatePdf,
    RateLimitError
} from './api.js';
import {
    renderCharacter,
    renderError,
    renderStatus
} from './renderers.js';

const generateButton = getRequiredElement('generateButton');
const createButton = getRequiredElement('createButton');
const sheetElement = getRequiredElement('characterSheet');
const downloadPdfButton = getRequiredElement('downloadPdfButton');
const conceptSelect = getRequiredElement('conceptSelect');
const clanSelect = getRequiredElement('clanSelect');
const natureSelect = getRequiredElement('natureSelect');
const demeanorSelect = getRequiredElement('demeanorSelect');
const nameInput = getRequiredElement('nameInput');
const generationSelect = getRequiredElement('generationSelect');
const ageCategorySelect = getRequiredElement('ageCategorySelect');
const ageInput = getRequiredElement('ageInput');
const progressContainer = getRequiredElement('progressContainer');
const progressBar = getRequiredElement('progressBar');
const debugToggle = getRequiredElement('debugToggle');
const debugContent = getRequiredElement('debugContent');
const debugList = getRequiredElement('debugList');
const debugArrow = getRequiredElement('debugArrow');
const contactToggle = getRequiredElement('contactToggle');
const contactContent = getRequiredElement('contactContent');
const contactArrow = getRequiredElement('contactArrow');
const customizeToggle = document.querySelector('.sidebar-toggle');
const customizeContent = document.querySelector('.sidebar-content');
const customizeArrow = document.querySelector('.arrow');

if (!customizeToggle || !customizeContent || !customizeArrow) {
    throw new Error('The customization accordion could not be initialized.');
}

let currentCharacterData = null;
let lastPdfUrl = null;
let allNatures = [];

generateButton.addEventListener('click', () => {
    renderStatus(sheetElement, 'Invoking the Blood...');
    fetchAndDisplayCharacter('/api/character/generate', { method: 'POST' });
});

createButton.addEventListener('click', () => {
    renderStatus(sheetElement, 'Embracing...');

    const age = ageInput.value ? Number.parseInt(ageInput.value, 10) : null;
    const generation = generationSelect.value
        ? Number.parseInt(generationSelect.value, 10)
        : null;
    const payload = {
        conceptId: conceptSelect.value,
        clanId: clanSelect.value,
        natureId: natureSelect.value,
        demeanorId: demeanorSelect.value,
        name: nameInput.value,
        generation,
        ageCategory: ageCategorySelect.value,
        age
    };

    fetchAndDisplayCharacter('/api/character/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
});

bindAccordion(customizeToggle, customizeContent, customizeArrow, {
    openPadding: '1.5rem',
    closedPadding: '0 1.5rem',
    getOpenHeight: () => `${customizeContent.scrollHeight}px`
});

bindAccordion(debugToggle, debugContent, debugArrow, {
    openPadding: '1rem',
    closedPadding: '0 1.5rem',
    getOpenHeight: () => '400px'
});

bindAccordion(contactToggle, contactContent, contactArrow, {
    openPadding: '0 1.5rem',
    closedPadding: null,
    getOpenHeight: () => `${contactContent.scrollHeight}px`
});

downloadPdfButton.addEventListener('click', downloadPdfWithProgress);
natureSelect.addEventListener('change', () => populateDemeanorOptions(natureSelect.value));
demeanorSelect.addEventListener('change', () => populateNatureOptions(demeanorSelect.value));

loadOptions();

async function fetchAndDisplayCharacter(url, options = {}) {
    try {
        const characterData = await fetchCharacter(url, options);
        currentCharacterData = characterData;

        downloadPdfButton.style.display = 'block';
        downloadPdfButton.textContent = '📄 Download Character Sheet (PDF)';
        downloadPdfButton.disabled = false;
        renderCharacter(sheetElement, debugList, characterData);
    } catch (error) {
        console.error('Error fetching character:', error);
        renderError(
            sheetElement,
            error instanceof RateLimitError
                ? error.message
                : 'The Embrace failed. Check console.'
        );
    }
}

async function downloadPdfWithProgress() {
    if (!currentCharacterData) {
        return;
    }

    downloadPdfButton.disabled = true;
    downloadPdfButton.textContent = '⏳ Contacting Scribe...';
    progressContainer.style.display = 'block';
    progressBar.style.width = '0%';
    progressBar.textContent = '0%';
    console.time('PDF Download');

    try {
        const pdfBlob = await generatePdf(currentCharacterData, updatePdfProgress);

        // Keep the previous URL alive until the next PDF is ready so repeat downloads remain reliable.
        if (lastPdfUrl) {
            window.URL.revokeObjectURL(lastPdfUrl);
        }

        lastPdfUrl = window.URL.createObjectURL(pdfBlob);
        const downloadLink = document.createElement('a');
        downloadLink.href = lastPdfUrl;
        const safeName = currentCharacterData.name
            ? currentCharacterData.name.replace(/\s+/g, '_')
            : 'Character';
        downloadLink.download = `${safeName}_Sheet.pdf`;

        // An off-screen attached link also works in browsers that ignore clicks on display:none elements.
        downloadLink.style.position = 'absolute';
        downloadLink.style.left = '-9999px';
        downloadLink.style.top = '0';
        document.body.appendChild(downloadLink);

        setTimeout(() => {
            downloadLink.click();
            downloadLink.remove();
            console.timeEnd('PDF Download');
            downloadPdfButton.disabled = false;
            downloadPdfButton.textContent = '✅ Done! Download Again?';
            progressContainer.style.display = 'none';
        }, 0);
    } catch (error) {
        console.timeEnd('PDF Download');
        console.error('Download failed:', error);
        downloadPdfButton.textContent = '❌ Error';
        progressBar.style.background = '#c92a2a';

        setTimeout(() => {
            downloadPdfButton.disabled = false;
            downloadPdfButton.textContent = '📄 Download Character Sheet (PDF)';
            progressBar.style.background = '#8c1c1c';
        }, 3000);
    }
}

function updatePdfProgress(progress) {
    if (progress.percent !== null) {
        progressBar.style.width = `${progress.percent}%`;
        progressBar.textContent = `${progress.percent}%`;
        return;
    }

    progressBar.style.width = '100%';
    progressBar.textContent = `${progress.receivedKilobytes} KB downloaded...`;
}

async function loadOptions() {
    try {
        const data = await fetchCharacterOptions();
        allNatures = data.natures || [];
        populateSelect(conceptSelect, data.concepts, 'Random');
        populateSelect(clanSelect, data.clans, 'Random');

        generationSelect.replaceChildren(createOption('', 'Random'));
        for (const generation of data.generations || []) {
            generationSelect.appendChild(
                createOption(generation, `${generation}th Generation`)
            );
        }

        populateNatureOptions();
        populateDemeanorOptions();
    } catch (error) {
        console.error('Error loading options:', error);
    }
}

function populateSelect(element, items, placeholder) {
    element.replaceChildren(createOption('', placeholder));
    for (const item of items || []) {
        element.appendChild(createOption(item.id ?? '', item.name ?? ''));
    }
}

function createOption(value, text) {
    const option = document.createElement('option');
    option.value = String(value);
    option.textContent = String(text);
    return option;
}

function populateDemeanorOptions(excludedId = '') {
    const previousValue = demeanorSelect.value;
    populateSelect(
        demeanorSelect,
        allNatures.filter((nature) => String(nature.id) !== String(excludedId)),
        'Random'
    );
    demeanorSelect.value = previousValue;
}

function populateNatureOptions(excludedId = '') {
    const previousValue = natureSelect.value;
    populateSelect(
        natureSelect,
        allNatures.filter((nature) => String(nature.id) !== String(excludedId)),
        'Random'
    );
    natureSelect.value = previousValue;
}

function bindAccordion(toggle, content, arrow, options) {
    toggle.addEventListener('click', () => {
        const isOpen = toggle.classList.toggle('active');
        arrow.textContent = isOpen ? '▲' : '▼';
        content.style.maxHeight = isOpen ? options.getOpenHeight() : null;

        if (isOpen) {
            content.style.padding = options.openPadding;
        } else if (options.closedPadding !== null) {
            content.style.padding = options.closedPadding;
        }
    });
}

function getRequiredElement(id) {
    const element = document.getElementById(id);
    if (!element) {
        throw new Error(`Required element #${id} was not found.`);
    }

    return element;
}
