const ATTRIBUTE_CATEGORIES = {
    Physical: ['strength', 'dexterity', 'stamina'],
    Social: ['charisma', 'manipulation', 'appearance'],
    Mental: ['perception', 'intelligence', 'wits']
};

const ABILITY_CATEGORIES = {
    Talents: ['alertness', 'athletics', 'awareness', 'brawl', 'empathy', 'expression', 'intimidation', 'leadership', 'streetwise', 'subterfuge'],
    Skills: ['animal_ken', 'crafts', 'drive', 'etiquette', 'firearms', 'larceny', 'melee', 'performance', 'stealth', 'survival'],
    Knowledges: ['academics', 'computer', 'finance', 'investigation', 'law', 'medicine', 'occult', 'politics', 'science', 'technology']
};

export function renderStatus(sheetElement, message) {
    const status = document.createElement('p');
    status.style.textAlign = 'center';
    status.style.marginTop = '2rem';
    status.textContent = message;
    sheetElement.replaceChildren(status);
}

export function renderError(sheetElement, message) {
    const errorMessage = document.createElement('p');
    errorMessage.style.color = '#ff6b6b';
    errorMessage.style.textAlign = 'center';
    errorMessage.textContent = message;

    sheetElement.classList.add('sheet-active');
    sheetElement.replaceChildren(errorMessage);
}

export function renderCharacter(sheetElement, debugListElement, characterData) {
    sheetElement.classList.add('sheet-active');
    sheetElement.replaceChildren(
        renderPersonaSection(characterData),
        renderAttributesSection(characterData.attributes || {}),
        renderAbilitiesSection(characterData.abilities || {}),
        renderDisciplinesSection(characterData.disciplines || {}),
        renderBackgroundsSection(characterData.backgrounds || {}),
        renderVirtuesSection(characterData.virtues || {}),
        renderCoreStatsSection(characterData),
        renderMeritsAndFlawsSection(characterData.merits, characterData.flaws)
    );

    updateDebugLog(debugListElement, characterData.debugLog);
}

function renderPersonaSection(data) {
    const container = document.createElement('div');
    container.className = 'sheet-section';

    const experienceDisplay = `${data.spentExperience || 0} / ${data.totalExperience || 0}`;
    appendHeading(container, 2, 'Persona');

    const grid = document.createElement('div');
    grid.className = 'persona-grid';
    grid.appendChild(createPersonaField('Name', data.name ?? 'Unknown Kindred'));
    grid.appendChild(createPersonaField('Concept', data.concept?.name ?? '—'));
    grid.appendChild(createPersonaField('Clan', data.clan?.name ?? '—'));
    grid.appendChild(createPersonaField('Generation', `${data.generation ?? '—'}th`));
    grid.appendChild(createPersonaField('Nature', data.nature?.name ?? '—'));
    grid.appendChild(createPersonaField('Age Category', data.ageCategory ?? '—'));
    grid.appendChild(createPersonaField('Demeanor', data.demeanor?.name ?? '—'));
    grid.appendChild(createPersonaField('Age', `${data.age ?? '0'} years`));

    const experienceField = createPersonaField('Experience', experienceDisplay);
    experienceField.style.gridColumn = 'span 2';
    experienceField.style.borderBottom = 'none';
    experienceField.style.marginTop = '5px';
    grid.appendChild(experienceField);
    container.appendChild(grid);

    return container;
}

function renderAttributesSection(attributes) {
    const section = createSection('Attributes');
    const grid = document.createElement('div');
    grid.className = 'attributes-grid';

    for (const [category, attributeIds] of Object.entries(ATTRIBUTE_CATEGORIES)) {
        const categoryElement = document.createElement('div');
        categoryElement.className = 'attribute-category';
        appendHeading(categoryElement, 3, category);

        for (const attributeId of attributeIds) {
            categoryElement.appendChild(createTraitItem(
                titleCaseAndClean(attributeId),
                attributes[attributeId] || 0,
                5,
                true
            ));
        }

        grid.appendChild(categoryElement);
    }

    section.appendChild(grid);
    return section;
}

function renderAbilitiesSection(abilities) {
    const section = createSection('Abilities');
    const grid = document.createElement('div');
    grid.className = 'attributes-grid';

    for (const [category, abilityIds] of Object.entries(ABILITY_CATEGORIES)) {
        const categoryElement = document.createElement('div');
        categoryElement.className = 'attribute-category';
        appendHeading(categoryElement, 3, category);

        for (const abilityId of abilityIds) {
            const score = Number(abilities[abilityId] || 0);
            const labelClass = score === 0 ? 'label muted' : 'label';
            categoryElement.appendChild(createTraitItem(
                titleCaseAndClean(abilityId),
                score,
                5,
                true,
                labelClass
            ));
        }

        grid.appendChild(categoryElement);
    }

    section.appendChild(grid);
    return section;
}

function renderDisciplinesSection(disciplines) {
    const section = createSection('Disciplines');
    const grid = document.createElement('div');
    grid.className = 'attributes-grid';

    const entries = Object.keys(disciplines)
        .filter((id) => disciplines[id] > 0)
        .map((id) => {
            let sortName = titleCaseAndClean(id);
            let sortOrder = 100;
            let displayName = sortName;
            const pathMatch = id.match(/(.*)\s*\((\d+)\)/);

            if (pathMatch) {
                sortName = titleCaseAndClean(pathMatch[1]);
                sortOrder = Number.parseInt(pathMatch[2], 10);
                displayName = sortOrder === 1
                    ? `${sortName} (Primary Path)`
                    : `${sortName} (Secondary Path)`;
            }

            return {
                sortName,
                sortOrder,
                displayName,
                score: disciplines[id]
            };
        })
        .sort((left, right) => {
            if (left.sortName < right.sortName) return -1;
            if (left.sortName > right.sortName) return 1;
            return left.sortOrder - right.sortOrder;
        });

    if (entries.length === 0) {
        grid.appendChild(createEmptyMessage('No Disciplines learned.', '#666'));
    } else {
        for (const entry of entries) {
            const item = createTraitItem(entry.displayName, entry.score, 5, true);
            item.style.width = '100%';
            grid.appendChild(item);
        }
    }

    section.appendChild(grid);
    return section;
}

function renderBackgroundsSection(backgrounds) {
    const section = createSection('Backgrounds');
    const container = document.createElement('div');
    container.className = 'items-column-container';

    const entries = Object.keys(backgrounds)
        .filter((id) => backgrounds[id] > 0)
        .sort();
    const entriesPerColumn = Math.ceil(entries.length / 3);
    const columns = [
        entries.slice(0, entriesPerColumn),
        entries.slice(entriesPerColumn, entriesPerColumn * 2),
        entries.slice(entriesPerColumn * 2)
    ];

    for (const columnEntries of columns) {
        const column = document.createElement('div');
        column.className = 'items-column';

        for (const id of columnEntries) {
            column.appendChild(createTraitItem(titleCaseAndClean(id), backgrounds[id], 5, false));
        }

        container.appendChild(column);
    }

    section.appendChild(container);
    return section;
}

function renderVirtuesSection(virtues) {
    const section = createSection('Virtues');
    const grid = document.createElement('div');
    grid.className = 'attributes-grid';

    for (const id of ['conscience', 'self_control', 'courage']) {
        grid.appendChild(createTraitItem(titleCaseAndClean(id), virtues[id] || 0, 5, false));
    }

    section.appendChild(grid);
    return section;
}

function renderCoreStatsSection(data) {
    const section = createSection('Core Stats');
    const container = document.createElement('div');
    const stats = document.createElement('div');
    stats.style.display = 'flex';
    stats.style.justifyContent = 'space-between';
    stats.style.marginBottom = '1rem';

    const humanityColumn = document.createElement('div');
    humanityColumn.style.flex = '1';
    humanityColumn.style.marginRight = '2rem';
    humanityColumn.appendChild(createTraitItem('Humanity', data.humanity || 0, 10, false));

    const willpowerColumn = document.createElement('div');
    willpowerColumn.style.flex = '1';
    willpowerColumn.appendChild(createTraitItem('Willpower', data.willpower || 0, 10, false));

    stats.append(humanityColumn, willpowerColumn);
    container.appendChild(stats);

    const bloodGrid = document.createElement('div');
    bloodGrid.className = 'persona-grid';
    bloodGrid.style.borderTop = '1px solid #444';
    bloodGrid.style.paddingTop = '1rem';
    bloodGrid.appendChild(createPersonaField('Max Blood Pool', data.maximumBloodPool ?? '—'));
    bloodGrid.appendChild(createPersonaField('Blood Per Turn', data.bloodPointsPerTurn ?? '—'));
    bloodGrid.appendChild(createPersonaField('Max Trait Rating', data.maxTraitRating ?? 5));
    container.appendChild(bloodGrid);

    section.appendChild(container);
    return section;
}

function renderMeritsAndFlawsSection(merits, flaws) {
    const normalizedMerits = merits || [];
    const normalizedFlaws = flaws || [];

    if (normalizedMerits.length === 0 && normalizedFlaws.length === 0) {
        return document.createElement('div');
    }

    const section = createSection('Merits & Flaws');
    const grid = document.createElement('div');
    grid.className = 'persona-grid';

    const meritColumn = document.createElement('div');
    appendHeading(meritColumn, 3, 'Merits');
    if (normalizedMerits.length > 0) {
        for (const merit of normalizedMerits) {
            meritColumn.appendChild(createCostItem(merit.name, `(${merit.cost} pts)`, '#888'));
        }
    } else {
        meritColumn.appendChild(createEmptyMessage('None'));
    }

    const flawColumn = document.createElement('div');
    appendHeading(flawColumn, 3, 'Flaws');
    if (normalizedFlaws.length > 0) {
        for (const flaw of normalizedFlaws) {
            flawColumn.appendChild(createCostItem(flaw.name, `(+${flaw.cost} pts)`, '#a82222'));
        }
    } else {
        flawColumn.appendChild(createEmptyMessage('None'));
    }

    grid.append(meritColumn, flawColumn);
    section.appendChild(grid);
    return section;
}

function updateDebugLog(debugListElement, logs) {
    debugListElement.replaceChildren();

    if (!logs || logs.length === 0) {
        const emptyLog = document.createElement('li');
        emptyLog.textContent = 'No logs generated.';
        debugListElement.appendChild(emptyLog);
        return;
    }

    for (const log of logs) {
        const item = document.createElement('li');
        item.textContent = log;
        item.style.borderBottom = '1px solid #333';
        item.style.padding = '4px 0';

        if (log.includes('[Flaw]')) item.style.color = '#ff6b6b';
        else if (log.includes('[Merit]')) item.style.color = '#51cf66';
        else if (log.includes('[XP]')) item.style.color = '#74c0fc';
        else if (log.includes('[Decay]')) item.style.color = '#ff922b';

        debugListElement.appendChild(item);
    }
}

function createSection(title) {
    const section = document.createElement('div');
    section.className = 'sheet-section';
    appendHeading(section, 2, title);
    return section;
}

function appendHeading(parent, level, text) {
    const heading = document.createElement(`h${level}`);
    heading.textContent = text;
    parent.appendChild(heading);
}

function createPersonaField(label, value) {
    const field = document.createElement('p');
    const labelElement = document.createElement('strong');
    labelElement.textContent = `${label}:`;
    field.appendChild(labelElement);
    field.appendChild(document.createTextNode(` ${String(value)}`));
    return field;
}

function createTraitItem(label, count, maxScale = 5, useElderLogic = true, labelClass = '') {
    const item = document.createElement('div');
    item.className = 'attribute-item';

    const labelElement = document.createElement('span');
    labelElement.className = labelClass;
    labelElement.textContent = label;

    const dots = document.createElement('span');
    dots.className = 'dots-display';
    dots.appendChild(renderDots(count, maxScale, useElderLogic));

    item.append(labelElement, dots);
    return item;
}

function createCostItem(name, costText, costColor) {
    const item = document.createElement('div');
    item.className = 'attribute-item';

    const nameElement = document.createElement('span');
    nameElement.textContent = name ?? '';

    const costElement = document.createElement('span');
    costElement.style.color = costColor;
    costElement.textContent = costText;

    item.append(nameElement, costElement);
    return item;
}

function createEmptyMessage(text, color = '#555') {
    const message = document.createElement('p');
    message.style.color = color;
    message.textContent = text;
    return message;
}

function renderDots(count, maxScale = 5, useElderLogic = true) {
    const fragment = document.createDocumentFragment();
    const numericCount = Number.isFinite(Number(count)) ? Number(count) : 0;

    if (!useElderLogic) {
        for (let index = 1; index <= maxScale; index++) {
            const dot = document.createElement('span');
            dot.className = index <= numericCount ? 'dot filled' : 'dot';
            fragment.appendChild(dot);
        }
        return fragment;
    }

    // Scores above five wrap around in red so elder ratings remain readable on a five-dot scale.
    const visualMaximum = 5;
    const redDotCount = numericCount > 5 ? numericCount - 5 : 0;

    for (let index = 1; index <= visualMaximum; index++) {
        let className = 'dot';

        if (numericCount <= 5) {
            if (index <= numericCount) className += ' filled';
        } else if (index <= redDotCount) {
            className += ' filled-special';
        } else {
            className += ' filled';
        }

        const dot = document.createElement('span');
        dot.className = className;
        fragment.appendChild(dot);
    }

    return fragment;
}

function titleCaseAndClean(value) {
    if (!value) return '';

    return value
        .replace(/_/g, ' ')
        .toLowerCase()
        .split(' ')
        .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
        .join(' ');
}
