import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MigraAutomaticoComponent } from './migra-automatico.component';

describe('MigraAutomaticoComponent', () => {
  let component: MigraAutomaticoComponent;
  let fixture: ComponentFixture<MigraAutomaticoComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MigraAutomaticoComponent]
    });
    fixture = TestBed.createComponent(MigraAutomaticoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
